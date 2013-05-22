/// <reference path="http://ajax.googleapis.com/ajax/libs/jquery/2.0.0/jquery.min.js" />

(function ($, window) {

    var postId, isNew, tools, toolbarButtons,
        txtTitle, txtContent, txtMessage,
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
        txtTitle.removeAttr('contentEditable');
        txtContent.removeAttr('contentEditable');

        btnNew.removeAttr("disabled");
        btnEdit.removeAttr("disabled");
        btnSave.attr("disabled", true);
        btnCancel.attr("disabled", true);
        $("#tools").fadeOut();

        if (isNew) {
            history.back();
        }
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
                execCommandOnElement(txtContent.get(0), command, link);
        }
        else if (command === "uploadImage") {
            showMessage(true, "Coming soon...");
        }
        else {
            execCommandOnElement(txtContent.get(0), command);
        }
    }

    function execCommandOnElement(el, commandName, value) {
        if (typeof window.getSelection != "undefined") {
            var sel = window.getSelection();

            // Save the current selection
            var savedRanges = [];
            for (var i = 0, len = sel.rangeCount; i < len; ++i) {
                savedRanges[i] = sel.getRangeAt(i).cloneRange();
            }

            // Temporarily enable designMode so that
            // document.execCommand() will work
            el.designMode = "on";

            // Select the element's content
            sel = window.getSelection();
            var range = sel.getRangeAt(0); // document.createRange();
            //range.selectNodeContents(el);
            //sel.removeAllRanges();
            sel.addRange(range);

            // Execute the command
            document.execCommand(commandName, false, value);

            // Disable designMode
            el.designMode = "off";

            // Restore the previous selection
            sel = window.getSelection();
            sel.removeAllRanges();
            for (var i = 0, len = savedRanges.length; i < len; ++i) {
                sel.addRange(savedRanges[i]);
            }
        }
        //else if (typeof document.body.createTextRange != "undefined") {
        //    // IE case
        //    var textRange = document.body.createTextRange();
        //    textRange.moveToElementText(el);
        //    textRange.execCommand(commandName, false, value);
        //}
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

        txtTitle.bind("blur", function () { toolbarButtons.removeAttr("disabled"); });
        txtTitle.bind("focus", function () { toolbarButtons.attr("disabled", true); });

        if (isNew) {
            editClicked();
        }
        else if (txtTitle !== null && txtTitle.length === 1 && location.pathname.length > 1) {
            btnEdit.removeAttr("disabled");
            btnDelete.removeAttr("disabled");
        }
    });

})(jQuery, window);