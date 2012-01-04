/*
ArcGIS Service Non-Direct-Connect Checker
Copyright (C) 2011 Washington State Department of Transportation

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>
*/

(function ($) {
    "use strict";

    // Create a table with a row total in the footer.
    $.widget("ui.totalTable", {
        _create: function () {
            var self = this, body, foot, row, cell, layerCount, columnCount; // Assume "this" is a table.

            body = $("tbody", self.element);
            layerCount = $("tr", body).length;
            columnCount = $("th", self.element).length;
            // Create the tfoot element and insert it before the body element.
            foot = $("<tfoot>").insertBefore(body);
            row = $("<tr>").addClass("total").appendTo(foot);
            $("<td>").text("Total").appendTo(row);
            $("<td>").addClass("total").attr({
                colspan: columnCount - 1
            }).text(String(layerCount)).appendTo(row);


            return self;
        }
    });

    $(document).ready(function () {
        var hidden = false, list, hostDiv;

        function toggleNonShared() {
            /// <summary>Toggles on or off all info for map services that are not in a folder called "Shared".</summary>
            $("[data-map-server]").filter(function () {
                return !$(this).data("map-server").match(/^Shared/)
            }).toggle();
        }

        function toggleInderectConnectionRows(evt) {
            /// <summary>Show or hide the direct connection rows.</summary>
            var button = evt.currentTarget;
            if (hidden) {
                $("*").show();
                hidden = false;
                $(button).text("Show only rows that are using an indirect connection");
            } else {
                $("tbody tr").hide();
                $("tr:has(.nondc)").show();
                $("div[data-map-server]:not(:has(tbody tr:visible))").hide();
                hidden = true;
                $(button).text("Show all");
            }
        }
        function toggleTBodies(evt) {
            /// <summary>Toggle the tbody elements.</summary>
            var visible;
            $("tbody,thead,dl").toggle();
            visible = $("tbody:visible").length > 0;
            $("#toggle-indirect-connect").attr({
                disabled: !visible
            });
            $(evt.currentTarget).text(visible ? "Show only totals" : "Show all details");

        }

        // Add a button that will filter results.
        $("<div>").attr({
            id: "buttonDiv"
        }).insertAfter("h1");

        $("<button>").attr({
            id: "toggle-indirect-connect"
        }).text("Show only rows that are using an indirect connection").appendTo("#buttonDiv").click(toggleInderectConnectionRows);
        $("<button>").text("Show only totals").appendTo("#buttonDiv").click(toggleTBodies);

        // Add a TOC
        hostDiv = $("<div>").attr("id", "hosts").insertBefore("script:first");
        list = $("<ul>").attr("id", "toc").appendTo(hostDiv);
        $("div[data-host]").each(function (index, hostDiv) {
            var id, li, mapServiceUl;
            var id = "host" + String(index);
            $(this).attr("id", id);
            li = $("<li>").appendTo(list);
            $("<a>").attr({ href: "#" + id }).text($(hostDiv).attr("data-host")).appendTo(li);
            $("<button>").attr({
                type: "button"
            }).text("toggle").addClass("toc-map-service-button").appendTo(li).click(function () {
                $("ol,ul", li).toggle("blind");
            });

            // Create map service ul
            mapServiceUl = $("<ul>").appendTo(li);
            $("div[data-map-server]", hostDiv).each(function (index, mapServerDiv) {
                var mapServerDivId = id + "map-server-" + index; // Create a unique id for the map service div.
                $(mapServerDiv).attr("id", mapServerDivId);
                $("<li>").append($("<a>").attr("href", "#" + mapServerDivId).text($(mapServerDiv).data("map-server"))).appendTo(mapServiceUl);
            });
        }).appendTo(hostDiv);

        // Add toggle TOC button.
        $("<button>").attr({
            type: "button"
        }).text("Toggle TOC").appendTo("#buttonDiv").click(function () {
            $("#toc").toggle("blind");
        });

        // Add toggle non-shared button
        $("<button>").attr({
            type: "button"
        }).text("Toggle non-\"shared\" services").appendTo("#buttonDiv").click(toggleNonShared);

        // Add totals to tables.
        $("table").filter(function () {
            return $("tbody > tr", this).length > 0;
        }).totalTable();

        // Hide the map service TOCs
        $("#toc > li > ul").hide();
    });
} (jQuery));
