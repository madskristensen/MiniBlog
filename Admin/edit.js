/// <reference path="http://ajax.googleapis.com/ajax/libs/jquery/2.0.0/jquery.min.js" />

(function ($, window) {

    var postId, isNew, tools, toolbarButtons,
        txtTitle, txtContent, txtMessage, txtImage,
        btnNew, btnEdit, btnDelete, btnSave, btnCancel;

    function newClicked(e) {
        location.href = "/new/";
    }

    function editClicked(e) {
        txtTitle.attr('contentEditable', true);
        txtContent.attr('contentEditable', true);
        txtContent.css({ minHeight: "400px" });

        btnNew.attr("disabled", true);
        btnEdit.attr("disabled", true);
        btnSave.removeAttr("disabled");
        btnCancel.removeAttr("disabled");

        $("#tools").fadeIn();
    }

    function saveClicked(e) {
        $.post("/admin/edit.ashx?mode=save", {
            id: postId,
            title: txtTitle.text(),
            content: txtContent.html()
        })
          .success(function (data) {
              if (isNew) {
                  location.href = "/" + data;
                  return;
              }

              showMessage(true, "The post was saved successfully");
              cancelClicked(e);
          })
          .fail(function (data) {
              if (data.status === 409)
                  showMessage(false, "The title is already in use");
              else
                  showMessage(false, "Something bad happened. Server reported " + data.status + " " + data.statusText);
          });
    }

    function cancelClicked(e) {
        if (isNew) history.back();

        txtTitle.removeAttr('contentEditable');
        txtContent.removeAttr('contentEditable');

        btnNew.removeAttr("disabled");
        btnEdit.removeAttr("disabled");
        btnSave.attr("disabled", true);
        btnCancel.attr("disabled", true);

        $("#tools").fadeOut();
    }

    function deleteClicked(e) {
        if (confirm("Are you sure you want to delete this post?")) {
            $.post("/admin/edit.ashx?mode=delete", { id: postId })
                .success(function (data) { location.href = "/"; })
                .fail(function (data) { showMessage(false, "Something went wrong. Please try again"); });
        }
    }

    function showMessage(success, message) {
        var className = success ? "alert-success" : "alert-error";
        txtMessage.addClass(className);
        txtMessage.text(message);
        txtMessage.parent().fadeIn();

        setTimeout(function () {
            txtMessage.parent().fadeOut("slow", function () {
                txtMessage.removeClass(className);
            });
        }, 4000);
    }

    function execCommand(e) {
        var command = $(this).attr("data-cmd");
        if (command === undefined)
            return;

        if (command === "createLink" || command === "insertImage") {
            var link = prompt("Please specify the link", "http://");
            if (link)
                execCommandOnElement(command, link);
        }
        else if (command === "source") {
            txtContent.text(txtContent.html());
            $(this).attr("data-cmd", "design");
        }
        else if (command === "design") {
            txtContent.html(txtContent.text());
            $(this).attr("data-cmd", "source");
        }
        else {
            execCommandOnElement(command);
        }
    }

    function execCommandOnElement(commandName, value) {
        var sel = window.getSelection();
        sel = window.getSelection();

        var range = sel.getRangeAt(0);
        sel.addRange(range);

        document.execCommand(commandName, false, value);
    }

    function handleFileUpload(evt) {
        var files = evt.target.files;

        for (var i = 0, f; f = files[i]; i++) {

            var reader = new FileReader();

            reader.onload = (function (theFile) {
                return function (e) {
                    $.post('/admin/edit.ashx?mode=upload', {
                        data: e.target.result,
                        name: theFile.name,
                        id : postId
                    })
                     .success(function (data) {
                         txtContent.focus();
                         insertHtmlAtCursor('<img alt="" src="' + data + '" />');
                     })
                    .fail(function (data){
                        showMessage(false, "Something bad happened. Server reported " + data.status + " " + data.statusText);
                    });
                };
            })(f);

            reader.readAsDataURL(f);
        }
    }

    function insertHtmlAtCursor(html) {
        var sel = window.getSelection();
        var range = sel.getRangeAt(0);
        var node = range.createContextualFragment(html);
        range.insertNode(node);
    }

    $(function () {
        $("body").css({ marginTop: "50px" });

        toolbarButtons = $("#tools button[data-cmd], #tools button[data-toggle]");
        //toolbarButtons.attr("disabled", true);

        isNew = location.pathname.replace(/\//g, "") === "new";
        postId = $("[itemprop~='blogpost']").attr("data-id");

        txtTitle = $("[itemprop~='blogpost'] [itemprop~='name']");
        txtContent = $("[itemprop~='articleBody']");
        txtMessage = $("#admin .alert");
        txtImage = $("#admin #txtImage");

        btnNew = $("#btnNew");
        btnEdit = $("#btnEdit");
        btnDelete = $("#btnDelete");
        btnSave = $("#btnSave");
        btnCancel = $("#btnCancel");

        btnNew.bind("click", newClicked);
        btnEdit.bind("click", editClicked);
        btnDelete.bind("click", deleteClicked);
        btnSave.bind("click", saveClicked);
        btnCancel.bind("click", cancelClicked);
        toolbarButtons.on("click", execCommand);
        txtImage.on("change", handleFileUpload);

        txtTitle.bind("blur", function () { toolbarButtons.removeAttr("disabled"); });
        txtTitle.bind("focus", function () { toolbarButtons.attr("disabled", true); });

        $('#btnUpload').click(function (e) {
            e.preventDefault();
            $('#txtImage').click();
        }
    );

        if (isNew) {
            editClicked();
        }
        else if (txtTitle !== null && txtTitle.length === 1 && location.pathname.length > 1) {
            btnEdit.removeAttr("disabled");
            btnDelete.removeAttr("disabled");
        }
    });

})(jQuery, window);