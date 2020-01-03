/// <reference path="jquery-1.9.1.js" />


window.xpTab = {
    orderId: 0,
    connectId:0,
    managenewtab: function (url) {
        openTabTo(url);
        return true;
    },
    updateTitle: function (tabId, title) {
        for (var k in window.xpTab.tabs) {
            var currtab = window.xpTab.tabs[k];
            if ((tabId + '') == currtab.tabId + '') {
                currtab.title = title;
                var span = $("#tablab_" + currtab.orderId + " .tab-label-link");
                span.attr('title', title + ' ' + currtab.url).html(title);
                break;
            }
        }
    },
    bindTab: function (tabId) {
        checkTabs();
    },
    tabs: []
};

function checkTabs() {
    var newtabs = [];
    var tabs = $(".main-tab li");
    for (var k = 0; k < tabs.length; k++) {
        var id = $(tabs[k]).data('connectid');
        var tabId = '';
        var thistab = getIframeWindow($('#tabpage_' + id));
        if (!thistab)
            continue;
        if (thistab.safeTab && thistab.safeTab.tabId) {
            tabId = thistab.safeTab.tabId;
            newtabs.push({
                tabId: tabId,
                title: thistab.document.title,
                url: thistab.location.href,
                orderId: id
            });
        } else {
            newtabs.push({
                tabId: '-1',
                title: thistab.document.title,
                url: thistab.location.href,
                orderId: id
            });
        }
    }
    xpTab.tabs = newtabs;
}

window.onresize = function () {
    resizefun();
}
$("a.navitem").each(function () {
    $(this).attr('href', $(this).attr('href').toLowerCase());
});
$(document).ready(function () {
    resizefun();
    $('a.navitem').click(function (e) {
        var url = $(this).attr('href');
        openTabTo(url);
        return false;
    });
    $('a.navitem').dblclick(function (e) {
        var url = $(this).attr('href');
        openTabTo(url, true);
        return false;
    });
    var newhash = (location.hash || '').replace('#', '');
    if (newhash.length > 0) {
        openTabTo(newhash);
    }
});

function openTabTo(url, refresh) {
    url = url.toLowerCase();
    window.xpTab.orderId++; 
    var f_container = $('#iframe_container li[data-urlkey="' + url + '"]');
    var needaddtablab = true;
    var curr_connectId = 0;
    if (f_container.length > 0) {
        curr_connectId = f_container.data('connectid');
        $('.main-tab li[data-urlkey="' + url + '"]').data("orderid", window.xpTab.orderId);
        if (refresh) {
            if (!safeCloseConfirm(f_container))
                return;
        }
        $('a.navitem').removeClass('active');
        $('a.navitem[href="' + url + '"]').addClass('active');
        $('.main-tab li').removeClass('active');
        $('.main-tab li[data-urlkey="' + url + '"]').addClass('active');
        $("#iframe_container li").hide();
        if (refresh) {
            f_container.remove();
            needaddtablab = false;
        } else {
            f_container.show();
            //var newnav = location.pathname + '#' + url;
            //location.href = newnav;
            return;
        }
    }
    if (!curr_connectId) {
        window.xpTab.connectId++;
        curr_connectId = window.xpTab.connectId;
    }
    var newiframe = '<li data-urlkey="{urlkey}" id="tabpage_{connectid}" data-connectid="{connectid}"><iframe src="{src}" width="100%" height="100%" frameborder="0" id="iframe_tab_{connectid}" data-connectid="{connectid}" name="iframe_tab_{id}"></iframe> </li>';
   // var newnav = location.pathname + '#' + url;
   // location.href = newnav;
    if ((url + "").indexOf('?') > 0) {
        newiframe = newiframe.replace('{src}', url + "&nocontainer=true");
    }
    else {
        newiframe = newiframe.replace('{src}', url + "?nocontainer=true");
    }
    newiframe = newiframe.replace('{urlkey}', url);
    newiframe = newiframe.replace(/\{id}/g, window.xpTab.orderId.toString());
    newiframe = newiframe.replace(/\{connectid}/g, curr_connectId.toString());
    $("#iframe_container li").hide();
    $("#iframe_container").append(newiframe);
    if (needaddtablab) {
        $('a.navitem').removeClass('active');
        var currmenuitem = $('a.navitem[href="' + url + '"]');
        var navlabtext = '';
        if (currmenuitem.length > 0) {
            navlabtext = currmenuitem.text();
        } else {
            navlabtext = "临时页面";
        }
        currmenuitem.addClass('active');
        $('.main-tab li').removeClass('active');
        var tablab = '<li data-urlkey="{urlkey}" class="tab-labbtn active" id="tablab_{connectid}" data-orderid="{id}"  data-connectid="{connectid}"><a onclick="tablab_click(this)"  class="tab-label-link" href="javascript:void(0)">{text}</a><a class="tab-close-icon" onclick="tablab_closeiconclick(this)">x</a></li>';
        tablab = tablab.replace("{text}", navlabtext)
            .replace("{urlkey}", url)
            .replace(/\{id}/g, window.xpTab.orderId.toString())
            .replace(/\{connectid}/g, curr_connectId.toString());
        $('.main-tab').append(tablab);
    }
}

function closeTab(url) {
    url = url.toLowerCase();
    var f_container = $('#iframe_container li[data-urlkey="' + url + '"]');
    if (f_container.length > 0) {
        if (!safeCloseConfirm(f_container))
            return;
        f_container.remove();
        $('.main-tab li[data-urlkey="' + url + '"]').remove();
        var tablabs = $('.main-tab li');
        if ($('.main-tab li.active').length == 0 && tablabs.length > 0) {
            var maxorderid = 0;
            var maxtab = tablabs[0];
            for (var k = 0; k < tablabs.length; k++) {
                var currorderid = parseInt($(tablabs[k]).data("orderid"));
                if (currorderid > maxorderid) {
                    maxorderid = currorderid;
                    maxtab = tablabs[k];
                }
            }
            var toshowli = $(maxtab);
            return openTabTo(toshowli.data('urlkey'));
        }
        return true;
    }
}

function showTab(connectid) {
    var tablab = $("#tablab_" + connectid);
    if (tablab.length == 0)
        return false;
    window.xpTab.orderId++;
    $('.main-tab li').removeClass('active');
    tablab.addClass("active").data("orderid", window.xpTab.orderId);
    $("#iframe_container li").hide();
    $("#tabpage_" + connectid).show();
}

function tablab_click(e) {
    var li = $(e).parents('li').first();
    openTabTo($(li).data("urlkey"));
    //showTab($(li).data("connectid"));
}

function tablab_closeiconclick(e) {
    var li = $(e).parents('li').first();
    closeTab($(li).data('urlkey'));
}

function resizefun() {
    var h_1 = $('.wrapper-header').height();
    var h_2 = window.innerHeight;
    $('.wrapper-main').height(h_2 - h_1 - 5);
}

function resize_tablab() {

    var fullshowwidth = window.innerWidth - 160 - 80;
    var tablabs = $(".main-tab li.tab-labbtn");
    var safewidth = 20;
    var eachwidth = 131;
    var allwidth = tablabs.length * eachwidth + safewidth;
    if (fullshowwidth > allwidth) {

    } else {
        if ($(".main-tab li.active").length == 0)
            return;

        for (var i = 0; i < tablabs.length; i++) {

        }
    }
}

function checkChildCanClose(iframeContainer) {
    var thistab = getIframeWindow(iframeContainer);
    if (!thistab)
        return true;
    if (thistab.safeTab && thistab.safeTab.checkEditing && thistab.safeTab.isEditing()) {
        return false;
    }
    return true;
}

function getIframeWindow(iframeContainer) {
    var iframe = $(iframeContainer).find('iframe');
    if (iframe.length == 0)
        return null;
    var thistab = document.getElementById(iframe.attr('id')).contentWindow;
    return thistab;
}

function safeCloseConfirm(ifrmeContainer) {
    if (!checkChildCanClose(ifrmeContainer)) {
        return confirm("文档在正编辑中，确定要退出吗？");
    }
    return true;
}