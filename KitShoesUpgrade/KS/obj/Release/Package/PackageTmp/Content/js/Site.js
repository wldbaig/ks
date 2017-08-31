$(function () {
    setTimezoneCookie();
});

function setTimezoneCookie() {

    var timezone_cookie = "timeZone";

    // if the timezone cookie not exists create one.
    if (!$.cookie(timezone_cookie)) {
            // create a new cookie
        $.cookie(timezone_cookie, new Date().getTimezoneOffset());

    }
    else {

        var storedOffset = parseInt($.cookie(timezone_cookie));
        var currentOffset = new Date().getTimezoneOffset();

        if (storedOffset !== currentOffset) {
            $.cookie(timezone_cookie, new Date().getTimezoneOffset());
            location.reload();
        }
    }
}