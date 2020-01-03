$(function () {
    TimeShow();
    $("#ischeck").click(function () {
        TimeShow();
    });
});
var maxsecs = 60;
var sendagin;
function TimeShow() {
    $("#spndown").text(maxsecs + "秒后刷新");
    if ($("#ischeck").is(':checked')) {
        var ttime = maxsecs;
        sendagin = setInterval(function () {
            ttime--;
            $("#spndown").text(ttime + "秒后刷新");
            if (ttime == 0) {
                location.reload();
            }
        }, 1000);
    }
    else {
        clearInterval(sendagin);
        $("#spndown").text(maxsecs + "秒后刷新");
    }
}