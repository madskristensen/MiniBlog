/// <reference path="bootstrap-wysiwyg.js" />

(function ($, window) {

    var postId, isNew, tools, toolbarButtons,
        txtTitle, txtContent, txtMessage, txtImage, isPublished,
        btnNew, btnEdit, btnDelete, btnSave, btnPublish, btnCancel,

    newPost = function (e) {
        location.href = "/post/new/";
    },
    editPost = function (e) {
        txtTitle.attr('contentEditable', true);
        txtContent.wysiwyg({ hotKeys: {}, activeToolbarClass: "active" });
        txtContent.css({ minHeight: "400px" });
        txtContent.focus();

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
                self.addClass("active");
                txtContent.text(txtContent.html());
            }
            else {
                self.attr("data-cmd", "source");
                self.removeClass("active");
                txtContent.html(txtContent.text());
            }
        });
    },
    savePost = function (e) {
        if ($(".source").attr("data-cmd") === "design") {
            $(".source").click();
        }

        txtContent.cleanHtml();

        $.post("/post.ashx?mode=save", {
            id: postId,
            title: txtTitle.text().trim(),
            content: txtContent.html()
        })
          .success(function (data) {
              if (isNew) {
                  location.href = data;
                  return;
              }

              showMessage(true, "The post was saved successfully");
              cancelEdit(e);
          })
          .fail(function (data) {
              if (data.status === 409)
                  showMessage(false, "The title is already in use");
              else
                  showMessage(false, "Something bad happened. Server reported " + data.status + " " + data.statusText);
          });
    },
    publishPost = function (e) {
        $.post("/post.ashx?mode=publish", {
            id: postId,
            publish: !isPublished,
        })
          .success(function (data) {
              isPublished = !isPublished;
              showMessage(true, isPublished ? "The post has been unpublished" : "The post is published");
              btnPublish.text(isPublished ? "Unpublish" : "Publish")
              cancelEdit(e);
          })
          .fail(function (data) {
              showMessage(false, "Something bad happened. Server reported " + data.status + " " + data.statusText);
          });
    },
    cancelEdit = function (e) {
        if (isNew) history.back();

        txtTitle.removeAttr('contentEditable');
        txtContent.removeAttr('contentEditable');
        btnCancel.focus();

        btnNew.removeAttr("disabled");
        btnEdit.removeAttr("disabled");
        btnSave.attr("disabled", true);
        btnCancel.attr("disabled", true);

        $("#tools").fadeOut();
    },
    deletePost = function (e) {
        if (confirm("Are you sure you want to delete this post?")) {
            $.post("/post.ashx?mode=delete", { id: postId })
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

    isNew = location.pathname.replace(/\//g, "") === "postnew";

    postId = $("[itemprop~='blogPost']").attr("data-id");
    isPublished = $("#admin").attr("data-ispublished") === "True";

    txtTitle = $("[itemprop~='blogPost'] [itemprop~='name']");
    txtContent = $("[itemprop~='articleBody']");
    txtMessage = $("#admin .alert");
    txtImage = $("#admin #txtImage");

    btnNew = $("#btnNew").bind("click", newPost);
    btnEdit = $("#btnEdit").bind("click", editPost);;
    btnDelete = $("#btnDelete").bind("click", deletePost);
    btnSave = $("#btnSave").bind("click", savePost);
    btnPublish = $("#btnPublish").bind("click", publishPost);
    btnCancel = $("#btnCancel").bind("click", cancelEdit);

    $(document).keyup(function (e) {
        if (e.keyCode == 46) // Delete key
            deletePost();
        else if (e.keyCode == 27) // ESC key
            cancelEdit();
    });

    $('.uploadimage').click(function (e) {
        e.preventDefault();
        $('#txtImage').click();
    });

    if (isNew) {
        editPost();
    }
    else if (txtTitle !== null && txtTitle.length === 1 && location.pathname.length > 1) {
        btnEdit.removeAttr("disabled");
        btnDelete.removeAttr("disabled");
        btnPublish.removeAttr("disabled");
    }

})(jQuery, window);