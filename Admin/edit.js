/// <reference path="http://ajax.googleapis.com/ajax/libs/jquery/2.0.0/jquery.min.js" />
/// <reference path="wysiwyg.js" />

(function ($) {

    var txtTitle, txtContent, btnSave, btnCancel;

    $(function () {
        
        txtTitle = $("[itemprop~='name']");
        txtContent = $("[itemprop~='articleBody']");        

        txtTitle.attr('contentEditable', true);
        txtContent.attr('contentEditable', true);

        btnSave = $("#btnSave");
        btnCancel = $("#btnCancel");

        btnCancel.bind("click", function (e) {
            location.href = location.href.replace("/edit/", "/").replace("/create/", "/");
        });
    });

})(jQuery);