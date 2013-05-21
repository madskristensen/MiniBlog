/// <reference path="http://ajax.googleapis.com/ajax/libs/jquery/2.0.0/jquery.min.js" />

(function ($) {

    var postId, isNew, tools,
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
        btnSave.attr("disabled");
        btnCancel.attr("disabled");
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
        txtMessage.css(success ? { color: "green" } : { color: "red" });
        txtMessage.text(message);
        txtMessage.fadeIn();

        setTimeout(function () {
            txtMessage.fadeOut();
        }, 5000);
    }

    function execCommand(e) {
        var command = $(this).attr("data-cmd");

        if (command === "createLink" || command === "insertImage") {
            var link = prompt("Please specify the link", "http://");
            if (link)
                document.execCommand(command, false, link);
        }
        else {
            document.execCommand(command);
        }
    }

    $(function () {
        $("body").css({ marginTop: "50px" });
        $("#tools button").on("click", execCommand);

        isNew = location.pathname.replace(/\//g, "") === "new";
        postId = $("[itemprop~='blogpost']").attr("data-id");

        txtTitle = $("[itemprop~='blogpost'] [itemprop~='name']");
        txtContent = $("[itemprop~='articleBody']");
        txtMessage = $("#admin .message");

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

        if (isNew) {
            editClicked();
        }
        else if (txtTitle !== null && txtTitle.length === 1 && location.pathname.length > 1) {
            btnEdit.removeAttr("disabled");
            btnDelete.removeAttr("disabled");
        }
    });

})(jQuery);