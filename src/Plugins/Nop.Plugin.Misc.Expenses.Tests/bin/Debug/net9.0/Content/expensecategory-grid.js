/*
 * Nop.Plugin.Misc.Expenses - Expense Category page behavior (TT-018).
 *
 * The grid is the stock nopCommerce DataTables grid (Table.cshtml, name "expensecategory-grid"). Its
 * inline edit (Edit / Update / Cancel) is the framework's own and posts only the Editable columns
 * (Id, Name, PerMonthLimit). This file adds, from outside and without touching core:
 *  - the "Add new" panel submit (field errors shown per input, grid reload, no navigation);
 *  - the current category type on the update request. The type is not editable and is not part of the
 *    framework's post, but the server rejects any update whose type differs from the stored one (a
 *    missing type counts as different), so the type is read from the row and appended;
 *  - the raw Per Month Limit as the inline edit input value (the cell shows the currency formatted text);
 *  - display of field errors returned by an inline update in a live region;
 *  - the "Edit stores and translations" modal (TT-023): per-row button, AJAX-loaded body, save, grid reload.
 *    Bootstrap's modal supplies the focus trap, Escape to close and the backdrop; this file moves focus into
 *    the dialog on open and returns it to the opening button on close.
 * No business rules live here: every rule is enforced server-side by ExpenseCategoryValidator.
 */
(function () {
    'use strict';

    var gridSelector = '#expensecategory-grid';
    var gridJsName = 'expensecategory_grid';
    var limitColumn = 'PerMonthLimit';
    var typeColumn = 'ExpenseCategoryTypeName';

    //server field name -> the Add panel's input id (the error container is "<id>-error")
    var addFieldMap = {
        Name: 'addName',
        PerMonthLimit: 'addPerMonthLimit',
        ExpenseCategoryTypeId: 'addType'
    };

    /**
     * Clears the field errors of the Add panel.
     * @param {jQuery} $panel - the Add panel container
     */
    function clearAddErrors($panel) {
        $panel.find('input, select').removeClass('is-invalid').removeAttr('aria-invalid');
        $panel.find('.field-validation-error').empty();
    }

    /**
     * Shows each server field error next to its Add panel input. Messages are inserted as text.
     * @param {jQuery} $panel - the Add panel container
     * @param {Object} fieldErrors - server property name -> localized message
     */
    function showAddErrors($panel, fieldErrors) {
        var $first = null;

        $.each(fieldErrors, function (field, message) {
            var id = addFieldMap[field];
            if (!id) {
                return;
            }

            var $input = $panel.find('#' + id);
            $input.addClass('is-invalid').attr('aria-invalid', 'true');
            $panel.find('#' + id + '-error').text(message);

            if (!$first) {
                $first = $input;
            }
        });

        if ($first) {
            $first.trigger('focus');
        }
    }

    /**
     * Writes a message into the page's polite status region (success) or alert region (errors).
     * @param {string} regionSelector - "#expensecategory-status" or "#expensecategory-grid-errors"
     * @param {Array} messages - plain text messages
     */
    function setRegion(regionSelector, messages) {
        var $region = $(regionSelector);
        $region.empty();

        $.each(messages, function (index, message) {
            $('<div></div>').text(message).appendTo($region);
        });
    }

    /**
     * Gets the localized generic error text (rendered by the page; the raw browser/HTTP status text is never shown).
     * @returns {string} the generic error text
     */
    function genericErrorText() {
        return $('#expensecategory-grid-errors').attr('data-generic-error') || '';
    }

    function clearRegions() {
        setRegion('#expensecategory-status', []);
        setRegion('#expensecategory-grid-errors', []);
    }

    //The framework's inline update calls the global display_nop_error with whatever the server returned.
    //A field-keyed response is shown in the alert region (the grid is redrawn right after, so there is no
    //row left to attach it to); anything else keeps the stock behavior (e.g. the localized TypeLocked text).
    var originalDisplayNopError = window.display_nop_error;

    window.display_nop_error = function (e) {
        if (e && e.fieldErrors) {
            var messages = [];
            $.each(e.fieldErrors, function (field, message) {
                messages.push(message);
            });
            setRegion('#expensecategory-grid-errors', messages);
            return;
        }

        if (typeof originalDisplayNopError === 'function') {
            originalDisplayNopError(e);
        }
    };

    //Append the row's current type to the update request (see the header comment). jQuery has already
    //serialized a plain-object body into a string when prefilters run, same as the time-log plugin.
    $.ajaxPrefilter(function (options) {
        if (!options.url || options.url.indexOf('ExpenseCategoryUpdate') === -1) {
            return;
        }
        if (typeof options.data !== 'string' || options.data.indexOf('ExpenseCategoryTypeId=') !== -1) {
            return;
        }

        var typeId = $('tr[editState="editState"] td[data-columnname="' + typeColumn + '"] [data-type-id]').attr('data-type-id');
        if (typeof typeId === 'undefined') {
            return;
        }

        options.data += (options.data.length > 0 ? '&' : '') + 'ExpenseCategoryTypeId=' + encodeURIComponent(typeId);
    });


    /**
     * Wires the "Edit stores and translations" modal. The body is loaded from the server on every open, so
     * it always shows the stored values; nothing is cached client-side.
     */
    function initStoresAndTranslationsModal() {
        var $modal = $('#expensecategory-stores-translations-modal');
        if ($modal.length === 0) {
            return;
        }

        var $body = $modal.find('#expensecategory-stores-translations-body');
        var openerId = null;

        function showModalErrors(messages) {
            var $region = $body.find('#expensecategory-stores-translations-errors');
            $region.empty();
            $.each(messages, function (index, message) {
                $('<div></div>').text(message).appendTo($region);
            });
        }

        //open from a row button (delegated: the grid redraws its rows)
        $(document).on('click', '.expensecategory-stores-btn', function () {
            var $button = $(this);
            openerId = $button.attr('data-id');
            clearRegions();

            $.ajax({
                cache: false,
                type: 'GET',
                url: $modal.attr('data-load-url'),
                data: { id: openerId },
                success: function (data) {
                    //a missing category comes back as { error: "..." } instead of the body markup
                    if (data && typeof data === 'object') {
                        setRegion('#expensecategory-grid-errors', [data.error || '']);
                        return;
                    }

                    $body.html(data);
                    $modal.modal('show');
                },
                error: function () {
                    setRegion('#expensecategory-grid-errors', [genericErrorText()]);
                }
            });
        });

        //focus the first control once the dialog is visible
        $modal.on('shown.bs.modal', function () {
            $body.find('select, input[type="text"]:not([readonly])').first().trigger('focus');
        });

        //drop the body and return focus to the opening button (the row may have been redrawn, so look it up by id)
        $modal.on('hidden.bs.modal', function () {
            $body.empty();
            if (openerId !== null) {
                $('.expensecategory-stores-btn[data-id="' + openerId + '"]').first().trigger('focus');
            }
        });

        //save
        $modal.on('submit', '#expensecategory-stores-translations-form', function (event) {
            event.preventDefault();

            var $form = $(this);
            var $save = $form.find('#expensecategory-stores-translations-save');

            $form.find('input').removeClass('is-invalid').removeAttr('aria-invalid');
            $form.find('.field-validation-error').empty();
            showModalErrors([]);
            $save.prop('disabled', true);

            //serialize() keeps every selected store as a repeated SelectedStoreIds field
            var token = $('input[name=__RequestVerificationToken]').val();
            var postData = $form.serialize() + (token ? '&__RequestVerificationToken=' + encodeURIComponent(token) : '');

            $.ajax({
                cache: false,
                type: 'POST',
                url: $modal.attr('data-save-url'),
                data: postData,
                success: function (data) {
                    if (data && data.fieldErrors) {
                        var $first = null;

                        //field errors are keyed by the posted input name, e.g. "Locales[1].Name"
                        $.each(data.fieldErrors, function (field, message) {
                            var $input = $form.find('input[name="' + field + '"]');
                            $input.addClass('is-invalid').attr('aria-invalid', 'true');
                            $form.find('#' + $input.attr('id') + '-error').text(message);
                            $first = $first || $input;
                        });

                        if ($first) {
                            $first.trigger('focus');
                        }
                        return;
                    }

                    if (data && data.error) {
                        showModalErrors([data.error]);
                        return;
                    }

                    $modal.modal('hide');
                    setRegion('#expensecategory-status', [$modal.attr('data-saved-text')]);
                    updateTable(gridSelector);
                },
                error: function () {
                    showModalErrors([genericErrorText()]);
                },
                complete: function () {
                    $save.prop('disabled', false);
                }
            });
        });
    }

    $(function () {
        initStoresAndTranslationsModal();

        var $panel = $('#expensecategory-add-panel');

        //The framework's edit input is filled from the cell markup. The limit cell holds formatted currency
        //text, so replace the input value with the raw value carried on the cell's data-raw attribute.
        //Wrapped at DOM ready: the framework's inline script defines editData_* after this file may load.
        var originalEdit = window['editData_' + gridJsName];
        if (typeof originalEdit === 'function') {
            window['editData_' + gridJsName] = function (rowElement, id) {
                clearRegions();
                originalEdit(rowElement, id);

                var rowData = window['editRowData_' + gridJsName] || [];
                var raw = $('<div></div>').html(rowData[limitColumn] || '').children().first().attr('data-raw');
                $(rowElement).find('td[data-columnname="' + limitColumn + '"] input.userinput').val(raw || '');

                //the activated Edit link was hidden by the framework, so move focus to the row's first input
                //instead of letting it fall back to the start of the page (WCAG 2.4.3)
                $(rowElement).find('input.userinput').first().trigger('focus');
            };
        }

        if ($panel.length === 0) {
            return;
        }

        var $save = $panel.find('#expensecategory-add-save');

        $save.on('click', function () {
            clearAddErrors($panel);
            clearRegions();
            $save.prop('disabled', true);

            var postData = {
                Name: $panel.find('#addName').val(),
                PerMonthLimit: $panel.find('#addPerMonthLimit').val(),
                ExpenseCategoryTypeId: $panel.find('#addType').val()
            };
            addAntiForgeryToken(postData);

            $.ajax({
                cache: false,
                type: 'POST',
                url: $panel.attr('data-insert-url'),
                data: postData,
                success: function (data) {
                    if (data && data.fieldErrors) {
                        showAddErrors($panel, data.fieldErrors);
                        return;
                    }
                    if (data) {
                        display_nop_error(data);
                        return;
                    }

                    $panel.find('#addName').val('');
                    $panel.find('#addPerMonthLimit').val('');
                    $panel.find('#addType').val('0');
                    setRegion('#expensecategory-status', [$panel.attr('data-added-text')]);
                    updateTable(gridSelector);
                },
                error: function () {
                    setRegion('#expensecategory-grid-errors', [genericErrorText()]);
                },
                complete: function () {
                    $save.prop('disabled', false);
                }
            });
        });

        //clear a field's error as soon as the user edits it again
        $panel.on('input change', 'input, select', function () {
            var $input = $(this);
            $input.removeClass('is-invalid').removeAttr('aria-invalid');
            $panel.find('#' + $input.attr('id') + '-error').empty();
        });
    });

    window.expenseCategoryGrid = {
        clearAddErrors: clearAddErrors,
        showAddErrors: showAddErrors
    };
})();
