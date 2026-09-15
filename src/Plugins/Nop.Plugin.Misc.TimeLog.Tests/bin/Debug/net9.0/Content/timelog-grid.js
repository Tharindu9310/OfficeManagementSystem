/*
 * Nop.Plugin.Misc.TimeLog - custom admin grid behavior (TT-013).
 *
 * This codebase's real admin grid mechanism (confirmed against Nop.Web/Areas/Admin/Views/Shared/Table.cshtml
 * and _Table.Definition.cshtml, and Nop.Plugin.Misc.RFQ's own admin views) is a jQuery DataTables-based
 * grid (DataTablesModel / ColumnProperty / RenderXxx server-side classes), NOT a Kendo UI grid - there is
 * no Kendo dependency anywhere in this 5.00 codebase's admin area. The stock inline-edit pattern requires
 * an explicit click on a per-row "Edit" pencil icon, then an explicit click on a "Confirm" (checkmark) icon
 * to actually save (see editData_<table>/confirmEditData_<table>/cancelEditData_<table>, generated per-grid
 * by Table.cshtml when DataTablesModel.UrlUpdate is set).
 *
 * TT-005/AC-4 explicitly asks for autosave-on-blur (no explicit "Update" click) - a deliberate deviation
 * from that stock pattern. This file layers a focusout handler on top of the framework-generated edit
 * inputs: once a Draft row has been put into edit mode (Edit pencil already clicked), leaving the row
 * (blur) auto-triggers the same "confirm" action the checkmark icon would - the user never has to click it.
 */
(function () {
    'use strict';

    /**
     * CHANGE REQUEST (post-TT-005/AC-4): a row's edits must only be saved when the user explicitly
     * clicks the Update (checkmark) button - autosave-on-blur was the original design (TT-005/AC-4:
     * "no explicit Update click needed"), but was reversed per user feedback: selecting a Date (now a
     * native date picker, ENH-009) or changing the Project dropdown was firing an immediate save before
     * the user had finished editing the rest of the row. This function - and the wireAutosaveOnBlur call
     * that wired it to the grid - are intentionally kept as dead code (not deleted) so the original
     * TT-005 behavior can be restored quickly if a future requirement brings it back; it is no longer
     * invoked anywhere in this file.
     * @param {string} tableSelector - jQuery selector for the grid's <table> element (e.g. '#timelog-grid')
     * @param {string} tableJsName - the grid Name with '-' replaced by '_' (matches Table.cshtml's ReplaceName),
     *                               used to find the generated buttonConfirm_<tableJsName><id> element.
     */
    function wireAutosaveOnBlur(tableSelector, tableJsName) {
        var $table = $(tableSelector);

        $table.on('focusout', 'td[data-columnname] input.userinput', function () {
            var $input = $(this);

            //defer so a click that moved focus to another input in the SAME edit row doesn't
            //prematurely save mid-edit - only save once focus has left the row entirely
            window.setTimeout(function () {
                var $row = $input.closest('tr');

                if ($row.find('input.userinput:focus').length > 0)
                    return;

                //client-side format guard for the Time (HH:mm) field - blocks autosave on an
                //obviously malformed value; the server independently re-validates the real 0-24 range
                var $timeInput = $row.find('td[data-columnname="TimeDisplay"] input.userinput');
                if ($timeInput.length > 0 && !/^([0-9]{1,2}):([0-5][0-9])$/.test($.trim($timeInput.val()))) {
                    $timeInput.addClass('is-invalid');
                    return;
                }
                $timeInput.removeClass('is-invalid');

                var $confirmBtn = $row.find('a[id^="buttonConfirm_' + tableJsName + '"]:visible');
                if ($confirmBtn.length > 0)
                    $confirmBtn.trigger('click');
            }, 150);
        });
    }

    /**
     * ENH-003: strictly enforces HH:mm as the user types (not just on blur) - strips any character that
     * isn't a digit or colon, auto-inserts the colon after the 2nd digit, and caps the length at 5
     * ("H:mm"/"HH:mm" both fit). This is a typing guard only; the existing focusout regex check above
     * (and the server's independent re-validation in TimeLogController) remain the authoritative checks.
     * @param {string} selector - jQuery selector/context for delegation (e.g. '#timelog-grid' or document)
     * @param {string} inputSelector - selector for the actual HH:mm text input(s) within `selector`
     */
    function wireTimeInputMask(selector, inputSelector) {
        $(selector).on('input', inputSelector, function () {
            var $input = $(this);
            var raw = $input.val();

            //keep only digits and colons
            var digitsAndColon = raw.replace(/[^0-9:]/g, '');

            //drop any colon beyond the first
            var firstColon = digitsAndColon.indexOf(':');
            if (firstColon !== -1) {
                digitsAndColon = digitsAndColon.substring(0, firstColon + 1) +
                    digitsAndColon.substring(firstColon + 1).replace(/:/g, '');
            }

            //auto-insert the colon once 2 hour digits have been typed and the user hasn't typed one yet
            if (firstColon === -1 && digitsAndColon.length > 2) {
                digitsAndColon = digitsAndColon.substring(0, 2) + ':' + digitsAndColon.substring(2);
            }

            //cap overall length to "HH:mm" (5 chars)
            digitsAndColon = digitsAndColon.substring(0, 5);

            if (digitsAndColon !== raw) {
                $input.val(digitsAndColon);
            }
        });
    }

    /**
     * ENH-004: applies a field-keyed error dictionary (as now returned by TimeLogController's
     * TimeLogInsert/TimeLogUpdate on validation failure - see ValidateTimeLogAsync) to specific inputs
     * inside a container, red-bordering each offending input and rendering its message directly beneath
     * it - no alert/toast. Clears automatically the next time the user edits that field.
     * @param {jQuery} $scope - container to search for each field's input (a form panel, or a grid <tr>)
     * @param {Object} fieldToSelector - map of server field name -> jQuery selector (relative to $scope)
     * @param {Object} fieldErrors - map of server field name -> message, as returned by the controller
     */
    function applyFieldErrors($scope, fieldToSelector, fieldErrors) {
        $.each(fieldErrors, function (field, message) {
            var selector = fieldToSelector[field];
            if (!selector)
                return;

            var $input = $scope.find(selector).first();
            if ($input.length === 0)
                return;

            $input.addClass('is-invalid');

            var $msg = $input.nextAll('.field-validation-error').first();
            if ($msg.length === 0) {
                //the Add Time Entry panel pre-declares a "#<id>-error" placeholder per field (see
                //List.cshtml); the grid's dynamically-created edit inputs don't have one, so create it
                var placeholderId = ($input.attr('id') || '') + '-error';
                $msg = $scope.find('#' + placeholderId);
                if ($msg.length === 0) {
                    $msg = $('<div class="field-validation-error text-danger"></div>').insertAfter($input.closest('.input-group').length ? $input.closest('.input-group') : $input);
                }
            }
            $msg.text(message).show();

            //clear this field's error the moment the user changes it again
            $input.one('input change', function () {
                $(this).removeClass('is-invalid');
                $msg.empty().hide();
            });
        });
    }

    /**
     * Clears every field error previously applied by applyFieldErrors within $scope.
     */
    function clearFieldErrors($scope) {
        $scope.find('.is-invalid').removeClass('is-invalid');
        $scope.find('.field-validation-error').empty().hide();
    }

    /**
     * Converts a decimal hours value to an HH:mm string, client-side, by round-tripping through
     * minutes (round(hours * 60)) - a UX-only mirror of the authoritative server-side conversion in
     * TimeLogController.ToTimeDisplay. The server always re-derives/re-validates independently.
     */
    function decimalHoursToHHmm(hours) {
        hours = parseFloat(hours);
        if (isNaN(hours))
            return '00:00';

        var totalMinutes = Math.round(hours * 60);
        var h = Math.floor(totalMinutes / 60);
        var m = totalMinutes % 60;

        return (h < 10 ? '0' : '') + h + ':' + (m < 10 ? '0' : '') + m;
    }

    /**
     * Parses an HH:mm string back to decimal hours, client-side. Returns null on invalid input -
     * callers must treat that as "do not submit", never silently default to 0.
     */
    function hhmmToDecimalHours(value) {
        if (!value || value.indexOf(':') === -1)
            return null;

        var parts = value.split(':');
        var h = parseInt(parts[0], 10);
        var m = parseInt(parts[1], 10);

        if (isNaN(h) || isNaN(m))
            return null;

        return h + (m / 60);
    }

    /**
     * CHANGE REQUEST (post-TT-005/AC-4, same reversal as wireAutosaveOnBlur above): this used to save a
     * Project change immediately on the dropdown's onchange event. Per user feedback, a row's edits -
     * Project included - must now only be posted when the Update (checkmark) button is explicitly
     * clicked; BUG-004's ajaxPrefilter already reads the select's current value at that moment, so no
     * separate save path is needed for Project any more. List.cshtml's renderProjectColumn no longer
     * wires this function to the select's onchange; kept here (unused) in case immediate-save-per-field
     * is ever wanted again for a specific column.
     * The full current row values travel with the <select> as a data-row attribute (JSON) so this
     * partial update never blanks out the row's other fields, if ever re-wired.
     */
    window.timeLogProjectChanged = function (selectEl) {
        var $select = $(selectEl);
        var row = $select.data('row');

        //ENH-004: clear any error left over from a previous attempt on this same select before retrying
        clearFieldErrors($select.parent());

        var postData = {
            Id: row.Id,
            ProjectId: $select.val(),
            Task: row.Task,
            Description: row.Description,
            Date: row.Date,
            TimeDisplay: row.TimeDisplay
        };
        addAntiForgeryToken(postData);

        $.ajax({
            url: $select.data('update-url'),
            type: 'POST',
            dataType: 'json',
            data: postData,
            success: function (data) {
                if (data && data.fieldErrors) {
                    //ENH-004: field-keyed - highlight the select itself (ProjectId) directly, red border +
                    //inline message, no alert/toast
                    applyFieldErrors($select.parent(), { ProjectId: $select }, data.fieldErrors);
                } else if (data) {
                    display_nop_error(data);
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert(errorThrown);
            }
        });
    };

    //ENH-004: the framework's own row-edit "confirm" ajax call (Table.cshtml's updateRowData_timelog_grid,
    //generated core code this plugin must not modify) always calls the *global* display_nop_error(data) on
    //success. Overriding it here - scoped to this page only, since timelog-grid.js is loaded only by
    //List.cshtml's footer script block - is the one way to redirect that specific response into inline
    //per-field errors without touching core Table.cshtml. Any response shape this plugin doesn't recognize
    //(fieldErrors absent) still falls through to the original alert/toast behavior unchanged.
    var originalDisplayNopError = window.display_nop_error;

    window.display_nop_error = function (e) {
        if (e && e.fieldErrors) {
            var $activeRow = $('tr[editState="editState"]');
            if ($activeRow.length === 0) {
                //no row is currently in edit mode (e.g. response arrived after the row was already closed) -
                //nothing sensible to attach the error to, so fall back rather than silently drop it
                if (typeof originalDisplayNopError === 'function')
                    originalDisplayNopError(e);
                return;
            }

            var fieldToSelector = {
                Task: 'td[data-columnname="Task"] input.userinput',
                Date: 'td[data-columnname="Date"] input.userinput',
                Time: 'td[data-columnname="TimeDisplay"] input.userinput',
                TimeDisplay: 'td[data-columnname="TimeDisplay"] input.userinput'
            };

            applyFieldErrors($activeRow, fieldToSelector, e.fieldErrors);
            return;
        }

        if (typeof originalDisplayNopError === 'function')
            originalDisplayNopError(e);
    };

    //BUG-004: Table.cshtml's core-generated confirmEditData_timelog_grid/updateRowData_timelog_grid
    //(the pencil-edit -> checkmark "Update" flow) builds its POST body purely by walking columnData_timelog_grid
    //for columns with Editable == true and reading a child <input> out of that cell - see
    //updateRowData_@(tableName) in Presentation/Nop.Web/Areas/Admin/Views/Shared/Table.cshtml. The Project
    //column has no native Editable EditType (ENH-007 - no dropdown EditType exists) and its cell holds a
    //<select>, not an <input>, so the framework's own collection loop never includes ProjectId in that
    //request; every pencil->checkmark "Update" click was posting without it, which TimeLogValidator's
    //ProjectRequired rule always rejects.
    //
    //Rather than reimplementing/duplicating updateRowData_timelog_grid's postData-building logic (which
    //would be a much more fragile, core-coupled change - the same risk ENH-007 explicitly declined to take
    //for the edit-mode-gating ask), this hooks in from outside via a jQuery ajaxPrefilter scoped to the
    //TimeLogUpdate URL only, and injects the currently-open edit row's selected ProjectId into the
    //already-serialized POST body before it goes out - no core file is touched.
    //
    //By the time an ajaxPrefilter callback runs, jQuery has already serialized a plain-object `data` into a
    //query string (jQuery.ajax serializes before invoking prefilters), so this appends "&ProjectId=..." to
    //that string rather than mutating an object. It only appends when ProjectId is missing, so this never
    //fires for timeLogProjectChanged's own onchange autosave or any other caller that already supplies
    //ProjectId explicitly (both already post it correctly today) - avoiding a duplicate/conflicting key.
    $.ajaxPrefilter(function (options) {
        if (!options.url || options.url.indexOf('TimeLogUpdate') === -1)
            return;

        if (typeof options.data !== 'string' || options.data.indexOf('ProjectId=') !== -1)
            return;

        var $editRow = $('tr[editState="editState"]');
        if ($editRow.length === 0)
            return;

        var $projectSelect = $editRow.find('td[data-columnname="ProjectName"] select');
        if ($projectSelect.length === 0)
            return;

        options.data += (options.data.length > 0 ? '&' : '') + 'ProjectId=' + encodeURIComponent($projectSelect.val());
    });

    //ENH-009: the Date cell's inline-edit input is a plain text box because this framework's
    //EditType enum (Nop.Web.Framework.Models.DataTables.EditType) only offers Number/Checkbox/String -
    //there is no Date option, and a codebase-wide search turned up no other admin grid anywhere using a
    //native date-picker for an inline-edit column, so there is no existing grid-edit convention to mirror.
    //What the codebase DOES already establish, in this very view, is a native HTML5 date input for date
    //entry (the Add Time Entry panel's #addDate, type="date") - so rather than pull in a new date-picker
    //library, this reuses that same convention: once a row's Date cell has been swapped to an <input> by
    //the framework's own setEditStateValue_timelog_grid (triggered by editData_timelog_grid), retype that
    //specific input to type="date" and normalize its value to yyyy-MM-dd (stripping any leftover "T..."
    //time portion), so the browser renders its native date picker instead of free-text entry. No time
    //portion is ever introduced (AC-3) since the value is always just the date part.
    //
    //Table.cshtml's updateRowData_timelog_grid reads this input the same way as any other Editable String
    //column (`.children('input')` then `.val()`), so a native type="date" input needs no special handling
    //there - a date input's .val() is already a plain "yyyy-MM-dd" string, which the server's TimeLogModel.Date
    //(DateTime) binds directly, same as it did with the plain text box before this change.
    function enhanceDateEditor($row) {
        var $dateInput = $row.find('td[data-columnname="Date"] input.userinput');
        if ($dateInput.length === 0)
            return;

        var raw = $.trim($dateInput.val());
        var datePart = raw.split('T')[0];

        $dateInput.attr('type', 'date').val(datePart);
    }

    /**
     * BUG-006 (follow-up): the Project <select> (renderProjectColumn in List.cshtml) is rendered
     * `disabled` by default so it matches every other column's edit-pencil-gated behavior - Project has
     * no native Editable EditType (ENH-007), so this enables/disables it manually from outside the
     * framework's own edit-state machinery, mirroring enhanceDateEditor's approach for the same reason.
     */
    function enableProjectEditor($row) {
        $row.find('td[data-columnname="ProjectName"] select').prop('disabled', false);
    }

    function disableProjectEditor($row) {
        $row.find('td[data-columnname="ProjectName"] select').prop('disabled', true);
    }

    //Wrap the framework-generated editData_timelog_grid/cancelEditData_timelog_grid (defined by an inline
    //<script> that Table.cshtml renders for this grid) so the Date input becomes a date picker and the
    //Project select is enabled/disabled correctly around the row's edit state - same "wrap the generated
    //function from outside, don't touch core" approach already used for display_nop_error (ENH-004/BUG-004).
    //
    //IMPORTANT ordering note (found while diagnosing ENH-009/BUG-006 never taking effect): Table.cshtml's
    //<script> block has no asp-location attribute, but Nop.Web.Framework's NopScriptTagHelper targets
    //*every* <script> tag, not just ones with asp-location explicitly set - when JavaScript bundling is
    //enabled (WebOptimizerConfig.EnableJavaScriptBundling), a location-less inline script still gets
    //auto-promoted to the Footer location and suppressed from its original position (see
    //NopScriptTagHelper.ProcessAsync: Location == Auto -> Footer when bundling is on). _AdminLayout.cshtml
    //then renders footer content in two separate passes - @NopHtml.GenerateScripts(Footer) (external
    //<script src> tags, which is what THIS file's own <script src="...timelog-grid.js"> tag becomes) runs
    //BEFORE @NopHtml.GenerateInlineScripts(Footer) (inline <script> blocks, which is what Table.cshtml's
    //editData_timelog_grid/cancelEditData_timelog_grid definitions become). So this file's top-level code
    //always executes *before* those functions exist when bundling is on - capturing
    //`window.editData_timelog_grid` at parse time silently found `undefined` and the wrap never applied,
    //even though window.timeLogGrid (this file's own export) was defined just fine.
    //
    //Fix: defer the wrap to jQuery's DOM-ready callback. By the time `ready` fires, the entire initial
    //HTML document (both footer passes, regardless of their relative order) has already been parsed and
    //executed, so both target functions are guaranteed to exist.
    $(function () {
        //Re-adjusts DataTables' column widths after this file's own DOM changes. Core's own
        //setEditStateValue_timelog_grid already calls `.columns.adjust()` once per swapped cell (see
        //Table.cshtml), but that happens *before* enhanceDateEditor/enableProjectEditor run (they're
        //invoked from our wrapper, after the original function returns) - retyping the Date input to
        //type="date" and enabling the Project <select> both change that cell's rendered width, and
        //without a further adjust() call the header row and body columns fall out of alignment the moment
        //a row enters edit mode. `false` (no redraw/no data refetch) keeps this a pure width recalculation.
        function readjustColumns() {
            var $table = $('#timelog-grid');
            if ($.fn.DataTable && $table.length > 0 && $.fn.DataTable.isDataTable($table)) {
                $table.DataTable().columns.adjust();
            }
        }

        var originalEditData_timelog_grid = window.editData_timelog_grid;
        if (typeof originalEditData_timelog_grid === 'function') {
            window.editData_timelog_grid = function (dataId, data) {
                originalEditData_timelog_grid(dataId, data);
                enhanceDateEditor(dataId);
                enableProjectEditor(dataId);
                readjustColumns();
            };
        }

        //Mirror wrap for cancelEditData_timelog_grid: core's own cancel handler restores the other
        //Editable columns' original values (saveArrayIntoRow_timelog_grid) but never touches the
        //non-Editable Project cell, so without this the enabled <select> would stay enabled after Cancel.
        //Core identifies the row being cancelled via the global `[editState=editState]` selector (not its
        //own dataId argument), so this captures that same row before the original handler clears the
        //attribute. Also re-adjusts columns afterward, since disabling the Project select and restoring
        //the Date cell back to plain text both change cell width again.
        var originalCancelEditData_timelog_grid = window.cancelEditData_timelog_grid;
        if (typeof originalCancelEditData_timelog_grid === 'function') {
            window.cancelEditData_timelog_grid = function (dataId, data) {
                var $editingRow = $('tr[editState="editState"]');
                originalCancelEditData_timelog_grid(dataId, data);
                disableProjectEditor($editingRow);
                readjustColumns();
            };
        }
    });

    window.timeLogGrid = {
        wireAutosaveOnBlur: wireAutosaveOnBlur,
        wireTimeInputMask: wireTimeInputMask,
        applyFieldErrors: applyFieldErrors,
        clearFieldErrors: clearFieldErrors,
        decimalHoursToHHmm: decimalHoursToHHmm,
        hhmmToDecimalHours: hhmmToDecimalHours
    };
})();
