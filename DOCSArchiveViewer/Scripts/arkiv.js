var Arkiv = (function (my, $) {
    my = {};

    my.searchResults = {};

    my.currentItem = {};

    my.query = {};

    my.search = function (callback) {
        $("html").addClass("wait");
        $.ajax({
            url: 'api/Search',
            type: 'POST',
            dataType: 'json',
            crossDomain: true,
            data: JSON.stringify(this.query),
            contentType: 'application/json',
            success: function (data) {
//                $(data).each(function (ix, item) {
//                    console.log(item.displayNameField);
//                });
                my.searchResults = data.archiveObjectField;
                callback(my.searchResults, data.totalCountField);
            },
            error: function (xhr, status, errorThrown) {
                alert(status);
            },
            complete: function (xhr, status) {
                $("html").removeClass("wait");
            }
        });
    }

    my.details = function (q, callback) {
        $("html").addClass("wait");
        $.ajax({
            url: 'api/ArchiveObject',
            type: 'POST',
            dataType: 'json',
            crossDomain: true,
            data: JSON.stringify(q),
            contentType: 'application/json',
            success: function (data) {
                my.currentItem = data;
                callback(my.currentItem);
            },
            error: function (xhr, status, errorThrown) {
                alert(status);
            },
            complete: function (xhr, status) {
                $("html").removeClass("wait");
            }
        });
    }

    return my;
}(Arkiv, jQuery));