(function ($) {

    function deleteComment(commentId, postId, element) {

        if (confirm("Do you want to delete this comment?")) {
            $.post("/comment.ashx?mode=delete", { postId: postId, commentId: commentId })
             .success(function (data) {
                 console.log(element);
                 element.slideUp();
             })
             .fail(function (data) {
                 alert("Something went wrong. Please try again");
             });
        }
    }

    function saveComment(name, email, content, postId, callback) {

        if (localStorage) {
            localStorage.setItem("name", name);
            localStorage.setItem("email", email);
        }

        $.post("/comment.ashx?mode=save", { postId: postId, name: name, email: email, content: content })
         .success(function (data) {
             $("#status").text("Your comment has been added").attr("class", "info");
             $("#commentcontent").val("");

             $.get(location.pathname, function (html) {
                 var comment = $(html).find("[data-id=" + data + "]").hide();
                 $("#comments").append(comment);
                 comment.slideDown();
                 callback(true);
             });
         })
         .fail(function (data) {
             $("#status").attr("class", "error").text("Remember to fill out all the fields");
             callback(false);
         });
    }

    function initialize() {

        var postId = $("[itemprop='blogPost']").attr("data-id");
        var email = $("#commentemail");
        var name = $("#commentname");
        var content = $("#commentcontent");

        $(document).on("submit", "#commentform", function (e) {
            e.preventDefault();
            var button = $(e.target);
            button.attr("disabled", true);

            saveComment(name.val(), email.val(), content.val(), postId, function (success) {
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

            if (name.val().length === 0)
                name.val(localStorage.getItem("name"));
        }
    }

    $(function () {
        if ($("#commentform").get(0))
            initialize();
    });

})(jQuery);