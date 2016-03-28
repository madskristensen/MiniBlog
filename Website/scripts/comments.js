/* globals NodeList, HTMLCollection */

(function () {
    var postId = null;

    //#region Helpers

    function objectToUrl(obj) {
        var string = '';

        for (var prop in obj) {
            if (obj.hasOwnProperty(prop)) {
                string += encodeURIComponent(prop) + '=' + encodeURIComponent(obj[prop]) + '&';
            }
        }

        string = string.substring(0, string.length - 1).replace(/%20/g, '+');

        return string;
    }

    var AsynObject = AsynObject ? AsynObject : {};

    AsynObject.ajax = function (url, callback) {
        var ajaxRequest = AsynObject.getAjaxRequest(callback);
        ajaxRequest.open("GET", url, true);
        ajaxRequest.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
        ajaxRequest.send(null);
    };

    AsynObject.postAjax = function (url, callback, data) {
        var ajaxRequest = AsynObject.getAjaxRequest(callback);
        ajaxRequest.open("POST", url, true);
        ajaxRequest.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        ajaxRequest.send(objectToUrl(data));
    };

    AsynObject.getAjaxRequest = function (callback) {

        var ajaxRequest = new XMLHttpRequest();

        ajaxRequest.onreadystatechange = function () {
            if (ajaxRequest.readyState > 1 && ajaxRequest.status > 0) {
                callback(ajaxRequest.readyState, ajaxRequest.status, ajaxRequest.responseText);
            }
        };

        return ajaxRequest;
    };

    function hasClass(elem, className) {
        return new RegExp(' ' + className + ' ').test(' ' + elem.className + ' ');
    }

    function addClass(elem, className) {
        if (!hasClass(elem, className)) {
            elem.className += ' ' + className;
        }
    }

    function removeClass(elem, className) {
        var newClass = ' ' + elem.className.replace(/[\t\r\n]/g, ' ') + ' ';
        if (hasClass(elem, className)) {
            while (newClass.indexOf(' ' + className + ' ') >= 0) {
                newClass = newClass.replace(' ' + className + ' ', ' ');
            }
            elem.className = newClass.replace(/^\s+|\s+$/g, '');
        }
    }

    function toDOM(htmlString) {
        var wrapper = document.createElement('div');
        wrapper.innerHTML = htmlString;
        return wrapper.children;
    }

    function getParentsByAttribute(element, attr, value) {
        var arr = [];

        while (element) {
            element = element.parentNode;
            if (element.hasAttribute(attr) && element.getAttribute(attr) === value) {
                arr.push(element);
            }
            if (!element.parentNode.parentNode) {
                break;
            }
        }

        return arr;
    }

    Element.prototype.remove = function () {
        this.parentElement.removeChild(this);
    };

    NodeList.prototype.remove = HTMLCollection.prototype.remove = function () {
        for (var i = 0, len = this.length; i < len; i++) {
            if (this[i] && this[i].parentElement) {
                this[i].parentElement.removeChild(this[i]);
            }
        }
    };

    function slide(thisObj, direction, callback) {
        if (direction === "Up") {
            thisObj.style.height = '0px';
        } else {
            var clone = thisObj.cloneNode(true);

            clone.style.position = 'absolute';
            clone.style.visibility = 'hidden';
            clone.style.height = 'auto';

            addClass(clone, 'slideClone col-md-6 col-md-offset-3');

            document.body.appendChild(clone);

            var slideClone = document.getElementsByClassName("slideClone")[0];
            var newHeight = slideClone.clientHeight;

            slideClone.remove();
            thisObj.style.height = newHeight + 'px';
            if (callback) {
                setTimeout(function () {
                    callback();
                }, 500);
            }
        }
    }

    //#endregion

    var endpoint = "/comment.ashx";

    function deleteComment(commentId, element) {

        if (confirm("Do you want to delete this comment?")) {
            AsynObject.postAjax(endpoint, function (state, status) {
                if (state === 4 && status === 200) {
                    slide(element, "Up", function () {
                        element.remove();
                    });
                    return;
                } else if (status !== 200) {
                    alert("Something went wrong. Please try again");
                }
            }, {
                mode: "delete",
                postId: postId,
                commentId: commentId,
                __RequestVerificationToken: document.querySelector("input[name=__RequestVerificationToken]").getAttribute("value")
            });
        }
    }

    function approveComment(commentId, element) {

        AsynObject.postAjax(endpoint, function (state, status) {
            if (state === 4 && status === 200) {                
                element.remove();
                return;
            } else if (status !== 200) {
                alert("Something went wrong. Please try again");
            }
        }, {
            mode: "approve",
            postId: postId,
            commentId: commentId,
            __RequestVerificationToken: document.querySelector("input[name=__RequestVerificationToken]").getAttribute("value")
        });
    }

    function saveComment(name, email, website, content, callback) {

        if (localStorage) {
            localStorage.setItem("name", name);
            localStorage.setItem("email", email);
            localStorage.setItem("website", website);
        }

        AsynObject.postAjax(endpoint, function (state, status, data) {

            var elemStatus = document.getElementById("status");
            if (state === 4 && status === 200) {
                elemStatus.innerHTML = "Your comment has been added";
                removeClass(elemStatus, "alert-danger");
                addClass(elemStatus, "alert-success");

                document.getElementById("commentcontent").value = "";

                var comment = toDOM(data)[0];
                comment.style.height = "0px";
                var elemComments = document.getElementById("comments");
                elemComments.appendChild(comment);
                slide(comment, "Down");
                callback(true);

                return;
            } else if (status !== 200) {
                addClass(elemStatus, "alert-danger");
                elemStatus.innerText = "Unable to add comment";
                callback(false);
            }
        }, {
            mode: "save",
            postId: postId,
            name: name,
            email: email,
            website: website,
            content: content,
            __RequestVerificationToken: document.querySelector("input[name=__RequestVerificationToken]").getAttribute("value")
        });

    }

    function initialize() {
        postId = document.querySelector("[itemprop=blogPost]").getAttribute("data-id");
        endpoint = document.getElementById("commentform").getAttribute("data-blog-path") + endpoint;
        var email = document.getElementById("commentemail");
        var name = document.getElementById("commentname");
        var website = document.getElementById("commenturl");
        var content = document.getElementById("commentcontent");
        var commentForm = document.getElementById("commentform");

        var allComments = document.querySelectorAll("[itemprop=comment]");
        for (var i = 0; i < allComments.length; ++i) {
            allComments[i].style.height = allComments[i].clientHeight + 'px';
        }

        commentForm.onsubmit = function (e) {
            e.preventDefault();
            var button = e.target;
            button.setAttribute("disabled", true);

            saveComment(name.value, email.value, website.value, content.value, function () {
                button.removeAttribute("disabled");
            });
        };

        website.addEventListener("keyup", function (e) {
            var w = e.target;
            if (w.value.trim().length >= 4 && w.value.indexOf("http") === -1) {
                w.value = "http://" + w.value;
            }
        });

        window.addEventListener("click", function (e) {
            var tag = e.target;

            if (hasClass(tag, "deletecomment")) {
                var comment = getParentsByAttribute(tag, "itemprop", "comment")[0];
                deleteComment(comment.getAttribute("data-id"), comment);
            }
            if (hasClass(tag, "approvecomment")) {
                var comment = getParentsByAttribute(tag, "itemprop", "comment")[0];
                approveComment(comment.getAttribute("data-id"), tag);
            }
        });

        if (localStorage) {
            email.value = localStorage.getItem("email");
            website.value = localStorage.getItem("website");

            if (name.value.length === 0) {
                name.value = localStorage.getItem("name");
            }
        }
    }

    if (document.getElementById("commentform")) {
        initialize();
    }
})();
