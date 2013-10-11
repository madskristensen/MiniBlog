(function ($) {

    var endpoint = "/comment.ashx";

    function deleteComment(commentId, postId, element) {

        if (confirm("Do you want to delete this comment?")) {
            $.post(endpoint, { mode: "delete", postId: postId, commentId: commentId })
             .success(function () {
                 element.slideUp(function () { element.remove(); });
             })
             .fail(function () {
                 alert("Something went wrong. Please try again");
             });
        }
    }

    function saveComment(name, email, website, content, postId, callback) {

        if (localStorage) {
            localStorage.setItem("name", name);
            localStorage.setItem("email", email);
            localStorage.setItem("website", website);
        }

        $.post(endpoint, { mode: "save", postId: postId, name: name, email: email, website: website, content: content })
         .success(function (data) {
             $("#status").text("Your comment has been added").attr("class", "info");
             $("#commentcontent").val("");

             $.get(data, function (html) {
                 var comment = $(html).hide();
                 $("#comments").append(comment);
                 comment.slideDown();
                 callback(true);
             });
         })
         .error(function (data) {
             $("#status").attr("class", "error").text(data.statusText);
             callback(false);
         });
    }

    function initialize() {

        var postId = $("[itemprop='blogPost']").attr("data-id");
        var email = $("#commentemail");
        var name = $("#commentname");
        var website = $("#commenturl");
        var content = $("#commentcontent");

        $(document).on("submit", "#commentform", function (e) {
            e.preventDefault();
            var button = $(e.target);
            button.attr("disabled", true);

            saveComment(name.val(), email.val(), website.val(), content.val(), postId, function () {
                button.removeAttr("disabled");
            });
        });

        $(document).on("click", ".deletecomment", function (e) {
            e.preventDefault();
            var button = $(e.target);
            var element = button.parents("[itemprop='comment']");
            deleteComment(element.attr("data-id"), postId, element);
        });

        if (localStorage) {
            email.val(localStorage.getItem("email"));
            website.val(localStorage.getItem("website"));

            if (name.val().length === 0)
                name.val(localStorage.getItem("name"));
        }
    }

    $(function () {
        if ($("#commentform").get(0))
            initialize();
    });

})(jQuery);
