/// <reference path="http://ajax.googleapis.com/ajax/libs/jquery/2.0.0/jquery.min.js" />
/// <reference path="bootstrap-wysiwyg.js" />

(function ($, window) {
    
    var postId, isNew, tools, toolbarButtons,
        txtTitle, txtContent, txtMessage, txtImage,
        btnNew, btnEdit, btnDelete, btnSave, btnCancel,

    newClicked = function (e) {
        location.href = "/new/";
    },
    editClicked = function (e) {
        txtTitle.attr('contentEditable', true);
        txtContent.wysiwyg({ activeToolbarClass: "active" });
        txtContent.css({ minHeight: "400px" });

        btnNew.attr("disabled", true);
        btnEdit.attr("disabled", true);
        btnSave.removeAttr("disabled");
        btnCancel.removeAttr("disabled");

        toggleSourceView();

        $("#tools").fadeIn().css("display", "inline-block");
    },
    toggleSourceView = function () {
        $(".source").bind("click", function (e) {
            var self = $(this);
            if (self.attr("data-cmd") === "source") {
                self.attr("data-cmd", "design");
                txtContent.text(txtContent.html());
            }
            else {
                self.attr("data-cmd", "source");
                txtContent.html(txtContent.text());
            }
        });
    },
    saveClicked = function (e) {
        if ($(".source").attr("data-cmd") === "design") {
            $(".source").click();
        }

        txtContent.cleanHtml();

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
    },
    cancelClicked = function (e) {
        if (isNew) history.back();

        txtTitle.removeAttr('contentEditable');
        txtContent.removeAttr('contentEditable');

        btnNew.removeAttr("disabled");
        btnEdit.removeAttr("disabled");
        btnSave.attr("disabled", true);
        btnCancel.attr("disabled", true);

        $("#tools").fadeOut();
    },
    deleteClicked = function (e) {
        if (confirm("Are you sure you want to delete this post?")) {
            $.post("/admin/edit.ashx?mode=delete", { id: postId })
                .success(function (data) { location.href = "/"; })
                .fail(function (data) { showMessage(false, "Something went wrong. Please try again"); });
        }
    },
    showMessage = function (success, message) {
        var className = success ? "alert-success" : "alert-error";
        txtMessage.addClass(className);
        txtMessage.text(message);
        txtMessage.parent().fadeIn();

        setTimeout(function () {
            txtMessage.parent().fadeOut("slow", function () {
                txtMessage.removeClass(className);
            });
        }, 4000);
    };

    $(function () {
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

        $('.uploadimage').click(function (e) {
            e.preventDefault();
            $('#txtImage').click();
        });

        if (isNew) {
            editClicked();
        }
        else if (txtTitle !== null && txtTitle.length === 1 && location.pathname.length > 1) {
            btnEdit.removeAttr("disabled");
            btnDelete.removeAttr("disabled");
        }
    });

})(jQuery, window);