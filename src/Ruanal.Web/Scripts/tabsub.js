/// <reference path="jquery-1.9.1.js" />

window.safeTab = {
    checkEditing: false,
    prepareData: null,
    tabId:  Math.random().toString().substr(2),
    _coreCheck: null,
    isEditing: function () {
        if (window.safeTab._coreCheck) {
            var issame = window.safeTab._coreCheck(window.safeTab.prepareData);
            return issame ? false : true;
        } else {
            return false;
        }
    },
    tabopen: function (url) {
        var strurl = '';
        if (typeof url === 'string') {
            strurl = url;
        } else {
            strurl = $(url).attr("href");
        }
        if (window.parent && window.parent.xpTab) {
            window.parent.xpTab.managenewtab(strurl);
            return true;
        } else {
            window.open(url);
        }
        return true;
    }
};

$(document).ready(function () {
    if (window.parent != window.self && window.parent.xpTab) {
        if (window.parent.xpTab.updateTitle) {
            window.parent.xpTab.updateTitle(window.safeTab.tabId, document.title);
        }
    }
});

function initSafeTab(ischeck, prepare, han) {
    window.safeTab.checkEditing = ischeck || false;
    if (window.safeTab.checkEditing && han) {
        window.safeTab._coreCheck = han;
    }
    if (prepare) {
        window.safeTab.prepareData = prepare();
    }
}

if (window.parent != window.self && window.parent.xpTab) {
    window.parent.xpTab.bindTab(window.safeTab.tabId);
}

// 刷新按扭
$(function () {
    $("#btn-refresh-iframepage").mousedown(function () {
        $("#btn-refresh-iframepage").unbind('mouseup');
        $("#btn-refresh-iframepage").mouseup(function () {
            if (window.safeTab.checkEditing && window.safeTab.isEditing()) {
                if (!confirm("你确定要取消编辑并刷新页面吗？"))
                    return;
            }
            document.location.reload();
        })
    })
    $("#btn-refresh-iframepage").draggable({
        start: function () {
            $("#btn-refresh-iframepage").unbind('mouseup');
        }, stop: function () {
            $("#btn-refresh-iframepage").unbind('mouseup');
        }, drag: function () {
            $("#btn-refresh-iframepage").unbind('mouseup');
        }
    });

});