(function ($) {

    function deleteComment(commentId, postId) {

        if (!confirm("Do you want to delete this comment?"))
            return;

        $.post("/comment.ashx?mode=delete", { postId: postId, commentId: commentId })
         .success(function (data) { location.reload(false); })
         .fail(function (data) { alert("Something went wrong. Please try again"); });
    }

    function saveComment(name, email, content, postId) {

        if (localStorage) {
            localStorage.setItem("name", name);
            localStorage.setItem("email", email);
        }

        $.post("/comment.ashx?mode=save", {
            postId: postId,
            name: name,
            email: email,
            content: content
        }).success(function (data) { location.reload(false); })
          .fail(function (data) { alert("Something went wrong. Please try again"); });
    }

    function initialize() {

        var postId = $("[itemprop='blogpost']").attr("data-id");
        var email = $("#commentemail");
        var name = $("#commentname");
        var content = $("#commentcontent");

        $("#commentform").bind("submit", function (e) {
            e.preventDefault();
            saveComment(name.val(), email.val(), content.val(), postId);
        });

        $(".deletecomment").bind("click", function (e) {
            e.preventDefault();
            var commentId = e.target.getAttribute("data-id");
            deleteComment(commentId, postId);
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