
String.prototype.supplant = function (o) {
    return this.replace(/{([^{}]*)}/g,
    function (a, b) {
        var r = o[b];
        return typeof r === 'string' || typeof r === 'number' ? r : '';
    }
    );
};

var itemTemplate = "<div class=\"list-group-item archive-item\" data-arkiv-id=\"{id}\" data-arkiv-secrecy=\"{secrecy}\"><h4 class=\"list-group-item-heading\">{display_name} <small>{arendetyp_docs}</small></h4><p class=\"list-group-item-text\">{arendemening_docs}</p></div>";

$(document).ready(function () {
    $("#searchButton").click(function () { search() });
    $("#display_name").keypress(function (e) {
        console.log(e.which);
        if (e.which == 13) search();
    });
    $("#more").click(function (e) { more(); });
    $(".dropdown-menu > li > a").click(function (e) {
        $("#display_name").attr("placeholder", searchTypes[e.target.id].text);
        searchType = searchTypes[e.target.id].type;
        $("#display_name").val("");
        $("#arkivbildare_row").toggle(searchType == "text");
    });
    $("#arkivbildare_row").toggle(searchType == "text");
});

var searchTypes = {
    "search_type_dnr": { "type": "dnr", "text": "Sök ärendenummer" },
    "search_type_text": { "type": "text", "text": "Sök ärendemening" }
};

var searchType = "dnr";

function updateQuery() {
    Arkiv.query = {};
    Arkiv.query.offset = 0;
    Arkiv.query.pagesize = 20;
    Arkiv.query.attributes = [];
    if ($("#display_name").val() != "") {
        if (searchType == "dnr") {
            Arkiv.query.attributes.push({
                "attribute": "display_name",
                "op": "MATCH",
                "value": $("#display_name").val()
            });
        } else {
            Arkiv.query.attributes.push({
                "attribute": "arendemening_docs",
                "op": "MATCH",
                "value": "*" + $("#display_name").val() + "*"
            });
            if ($("#arkivbildare_docs") != "") {
                Arkiv.query.attributes.push({
                    "attribute": "display_name",
                    "op": "MATCH",
                    "value": $("#arkivbildare_docs").val() + "*"
                });
            }
        }
    }
    if ($("#created_from").val() != "") {
        Arkiv.query.attributes.push({
            "attribute": "skapad_docs",
            "op": "GREATER_OR_EQUAL",
            "value": $("#created_from").val()
        });
    }
    if ($("#created_to").val() != "") {
        Arkiv.query.attributes.push({
            "attribute": "skapad_docs",
            "op": "LESS_OR_EQUAL",
            "value": $("#created_to").val()
        });
    }
};

function search() {
    updateQuery();
    Arkiv.search(displayNewResults);
}

function more() {
    if (Arkiv.query.attributes) {
        Arkiv.query.offset = Arkiv.query.offset + Arkiv.query.pagesize;
        Arkiv.search(displayResults);
    }
}

function displayNewResults(results, totalCount) {
    $("#results").html("");
    if (results) {
        $("#results").append("<p><i>" + totalCount + " träff" + (totalCount > 1 ? "ar" : "") + "</i></p>");
    }
    displayResults(results, totalCount);
}


function displayResults(results, totalCount) {
    $("#noHits").fadeOut(100);
    if (results) {
        $(results).each(function (ix, item) {
            var outItem = {};
            outItem.display_name = item.displayNameField;
            outItem.id = item.idField;
            $(item.attributeField).each(function (ix, item) {
                outItem[item.nameField] = item.valueField[0];
            });
            if (outItem.secrecy != '0') {
                outItem.arendemening_docs = 'Ärendet innehåller sekretess och/eller personuppgifter. Var vänlig och kontakta kommunarkivet via e-post <a href="mailto:kommunarkivet@kungsbacka.se">kommunarkivet@kungsbacka.se</a> eller telefon 0300-83 47 98.';
            }
            $("#results").append(itemTemplate.supplant(outItem));
        });
        $(".archive-item[data-arkiv-secrecy=0]").click(function (evt) {
            getDetails(this.dataset.arkivId);
        });
        $(".archive-item[data-arkiv-secrecy=0]").hover(function (evt) {
            $(this).addClass("btn-info cursor-pointer");
        }, function (evt) { $(this).removeClass("btn-info cursor-pointer"); });
        if (results.length == Arkiv.query.pagesize) {
            $("#moreRow").toggle(true);
        } else {
            $("#moreRow").toggle(false);
        }
    } else {
        $("#noHits").fadeIn(100);
        $("#moreRow").toggle(false);
    }
}

function getDetails(id) {
    var query = { "id": id };
    Arkiv.details(query, displayDetails);
}

var tableTemplate = "<tr><td>{sort_docs}</td><td>{skapad_docs}</td><td>{handlingsbeskrivning}</td><td>{status_docs}</td><td>{handlaggare_docs}</td><td>{filer}</td></tr>";

function displayDetails(data) {

    $("#det_display_name").html(data.display_name);
    $("#det_skapat_docs").html(data.skapad_docs.replace(/^(\d{4}-\d{2}-\d{2}).*/, "$1"));
    $("#det_arendemening_docs").html(data.arendemening_docs);
    $("#det_handlaggare_docs").html(data.handlaggare_docs);

    var tableData = "<tr><th>Sort</th><th>Datum</th><th>Beskrivning</th><th>Status</th><th>Handläggare</th><th>Filer</th></tr>";
    $(data.items).each(function (ix, item) {
        if (item.object_type == 'docs_handling') {
            var handelse = {};
            handelse["display_name"] = item.handlingsbeskrivning;
            handelse["sort_docs"] = parseInt(item.display_name.replace(/^(\d{2,4}).*$/, "$1")).toString();
            handelse["status_docs"] = item.status_docs;
            handelse["skapad_docs"] = (item.skapad_docs ? item.skapad_docs.replace(/^(\d{4}-\d{2}-\d{2}).*$/, "$1") : " ");
            handelse["handlingsbeskrivning"] = item.handlingsbeskrivning;
            handelse["handlaggare_docs"] = item.handlaggare_docs;
            handelse["filer"] = "";
            if (item.Files) {
                $(item.Files).each(function (ix, item) {
                    handelse["filer"] += '<a href="api/File?id=' + encodeURIComponent(item.id) + '" class="archiveFile">' + item.display_name + '</a>';
                });
            }
            tableData += tableTemplate.supplant(handelse);
        }
    });
    $("#det_handelser").html("<table class=\"table\">" + tableData + "</table>");
    $("#resultsContainer").addClass("hidden-print");
    $("#details").modal("show");
    $('#details').on('hidden.bs.modal', function (e) {
        $("#resultsContainer").removeClass("hidden-print");
    })
}
