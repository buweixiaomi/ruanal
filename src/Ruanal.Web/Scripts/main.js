///<reference path="~/Scripts/jquery-1.9.1.js" />
///<reference path="~/Scripts/main.js" />
///<reference path="~/Scripts/jquery-ui/jquery-ui.js" />
///<reference path="~/Scripts/jquery-ui/jquery-ui-timepicker-addon.js" />
Date.prototype.format = function (format) {
    var o = {
        "M+": this.getMonth() + 1,
        "d+": this.getDate(),
        "h+": this.getHours(),
        "m+": this.getMinutes(),
        "s+": this.getSeconds(),
        "q+": Math.floor((this.getMonth() + 3) / 3),
        "S": this.getMilliseconds()
    }
    if (/(y+)/.test(format)) format = format.replace(RegExp.$1,
        (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    for (var k in o) if (new RegExp("(" + k + ")").test(format))
        format = format.replace(RegExp.$1,
            RegExp.$1.length == 1 ? o[k] :
                ("00" + o[k]).substr(("" + o[k]).length));
    return format;
}

var setCookie = function (name, value) {
    var Days = 30;
    var exp = new Date();
    exp.setTime(exp.getTime() + Days * 24 * 60 * 60 * 1000);
    document.cookie = name + "=" + escape(value) + ";path=/;expires=" + exp.toGMTString();
}
function setvtheme(tname) {
    setCookie('vtheme', tname);
    location.reload();
}
function getCookie(name) {
    if (document.cookie.length > 0) {
        c_start = document.cookie.indexOf(name + "=")
        if (c_start != -1) {
            c_start = c_start + name.length + 1
            c_end = document.cookie.indexOf(";", c_start)
            if (c_end == -1) c_end = document.cookie.length
            return unescape(document.cookie.substring(c_start, c_end))
        }
    }
    return ""
}

var libjs = {
    lastid: 0
};

libjs.tabopen = function (url) {
    var strurl = '';
    if (typeof url === 'string') {
        strurl = url;
    } else {
        strurl = $(url).attr("href");
    }
    if (window.safeTab && window.safeTab.tabopen) {
        window.safeTab.tabopen(strurl);
        return false;
    }
    if (window.parent && window.parent.xpTab) {
        window.parent.xpTab.managenewtab(strurl);
        return false;
    }
    return true;
}

libjs.closedialog = function (id) {
    $('#' + id).modal('hide');
}

libjs.showdialog = function (title, content, btntext, callback) {
    libjs.lastid++;
    var lastid = libjs.lastid;
    var templa = '<div class="modal" id="dialog_@dialogid" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">' +
'     <div class="modal-dialog">' +
'         <div class="modal-content">' +
'             <div class="modal-header">' +
'                 <button type="button" class="close" data-dismiss="modal" aria-hidden="true">×</button>' +
'                <h4 class="modal-title" id="myModalLabel">@title</h4>' +
'            </div>' +
'            <div class="modal-body" id="m_value">@content</div>' +
'            <div class="modal-footer">' +
'                <button type="button" class="btn btn-default" id="closebtn_@dialogid">关闭</button>' +
 '               <button type="button" class="btn btn-primary" id="okbtn_@dialogid">@buttontext</button>' +
'           </div>' +
 '       </div>' +
    '    </div>' +
' </div> ';
    templa = templa.replace(/@dialogid/g, lastid).replace("@buttontext", btntext).replace("@title", title).replace("@content", content);
    var d = $(templa);
    $(document.body).append(d);
    $("#closebtn_" + lastid).click(function () {
        $('#dialog_' + lastid).modal('hide');
    });
    $("#okbtn_" + lastid).click(function () {
        var cc = callback("dialog_" + lastid);
        if (cc) {
            $('#dialog_' + lastid).modal('hide');
        }
    });
    $('#dialog_' + lastid).modal({
        keyboard: true
    })
    $('#dialog_' + lastid).on('hidden.bs.modal', function () {
        $('#dialog_' + lastid).remove();
    })
    return "dialog_" + lastid;
}

libjs.showselect = function (ty, callback, data, multy, curritems, showkeywords) {
    var url = "/comm/select?type=" + ty;
    data = data || {};
    var ajaxgetdata = function (url, tdata, callback1) {
        $.ajax({
            url: url,
            data: tdata,
            type: 'post',
            success: function (data1) {
                callback1(true, data1);
            }, error: function () {
                callback1(false, '请求出错！')
            }
        });
    };
    var selectcallback = function (id) {
        if ($("#" + id + " .active").length == 0) {
            alert("请选择");
            return false;
        }
        if (multy) {
            var selectitems = [];
            $("#" + id + " li.active").each(function () {
                var currid = $(this).data("item");
                for (var ti = 0; ti < selectitems.length; ti++) {
                    if (selectitems[ti].value == currid)
                        return;
                }

                selectitems.push({
                    value: $(this).data("item"),
                    text: $(this).data("text"),
                    type: $(this).data("type")
                });
            });
            callback(selectitems);
            return true;
        } else {
            var currseletitem = $("#" + id + " .active").first();
            var val = $(currseletitem).data("item");
            var text = $(currseletitem).data("text");
            var ty = $(currseletitem).data("type");
            callback(ty, text, val);
            return true;
        }
    }
    var inhtml = '<div class="search_con" style="margin:0 0 10px 0;"><label class="cc-block cc-width-0-6 text-center text-info">关键字:</label>' +
        '<input type="text" class="form-control cc-block cc-width-2-5 keywords" /><button class="btn btn-primary btn-search">查询</button></div><div class="content_con">正在加载...</div>';
    var diaid = libjs.showdialog("选择", inhtml, "确定", function (id) {
        if (selectcallback(id)) {
            libjs.closedialog(id);
        }
    });
    if (!showkeywords) {
        $("#" + diaid + " .search_con").hide();
    }
    var ajaxcallback = function (a, b) {
        $("#" + diaid + " .content_con").html(b);

        if (!(typeof curritems === 'string'))
            curritems = "";
        var curr_array = curritems.split(',');

        for (var ci = 0; ci < curr_array.length; ci++) {
            $("#" + diaid + " li").each(function () {
                if ($(this).data('item') == curr_array[ci]) {
                    $(this).addClass('active');
                }
            });
        }
        if (multy) {
            $("#" + diaid + " li").click(function () {
                var currselectv = $(this).data("item");
                if ($(this).hasClass("active")) {
                    //$(this).removeClass("active");
                    $("#" + diaid + " li").each(function (i, v) {
                        if ($(v).data("item") == currselectv) {
                            $(v).removeClass("active");
                        }
                    });
                }
                else {
                    //$(this).addClass("active");
                    //$(this).data("type", ty);
                    $("#" + diaid + " li").each(function (i, v) {
                        if ($(v).data("item") == currselectv) {
                            $(v).addClass("active");
                            $(v).data("type", ty);
                        }
                    });
                }

            })
        }
        else {
            $("#" + diaid + " li").click(function () {
                $("#" + diaid + " li").removeClass("active");
                var currselectv = $(this).data("item");
                $("#" + diaid + " li").each(function (i, v) {
                    if ($(v).data("item") == currselectv) {
                        $(v).addClass("active");
                        $(v).data("type", ty);
                    }
                });
            })
        }
    }
    $("#" + diaid + " .btn-search").click(function () {
        data.keywords = $("#" + diaid + " .keywords").val();
        ajaxgetdata(url, data, ajaxcallback)
    });
    ajaxgetdata(url, data, ajaxcallback);
}

libjs.getserverproject = function (pid, target, tags) {
    if (!pid)
        return;
    $.ajax({
        url: '/cmd/getserverproject',
        data: { projectid: pid, tags: tags },
        type: 'post',
        success: function (data) {
            var html = '<ul class="list-group"> ';
            for (var i = 0; i < data.data.length; i++) {
                html += '<li class="list-group-item"><input type="checkbox" name="serverprojectid" checked="checked" class="serverproitem" data-id="' + data.data[i].ServerProjectId + '" />' + data.data[i].Title + '</li>';
            }
            html += '</ul>';
            $(target).html(html);
        }
    });
}

libjs.getservers = function (target) {
    $.ajax({
        url: '/cmd/getservers',
        data: {},
        type: 'post',
        success: function (data) {
            var html = '<ul class="list-group"> ';
            for (var i = 0; i < data.data.length; i++) {
                html += '<li class="list-group-item"><input type="checkbox" checked="checked" class="container_servers" data-id="' + data.data[i].ServerId + '" />' + data.data[i].ServerName + '</li>';
            }
            html += '</ul>';
            $(target).html(html);
        }
    });
}

var libfincate = {};
libfincate.selectparcate = function () {
    libjs.showselect('financate', function (ty, txt, v) {
        $("#ParentCateId").val(v);
        $("#show_parentid").text(v + "-" + txt);
    });
}
libfincate.selectIndex = function () {
    libjs.showselect('postcatelabel', function (ty, txt, v) {
        $("#CateId").val(v);
        //$("#show_parentid").val(v + "-" + txt);
    });
}
libfincate.selectIndexConfig = function (e) {
    libjs.showselect('postcatelabel', function (ty, txt, v) {
        $(e).next().val(v);
        $(e).next().next().val(v + "-" + txt);
    });
}
libfincate.selectmultycate = function () {
    var curritem = $("#categoryid").val();
    libjs.showselect('postcatelabel', function (items) {
        var strid = "";
        var strname = "";
        $.each(items, function (key, name) {
            strid += name.value + ",";
            strname += name.text + ",";
        })
        $("#categoryid").val(strid);
        $("#show_parentid").text(strname);
    }, null, true, curritem);
}
var libcusservice = {};
libcusservice.index_selectcus = function () {

    libjs.showselect('customer', function (ty, txt, v) {
        $("#search_cusid").val(v);
    })
}
libcusservice.edit_selectcus = function () {
    libjs.showselect('customer', function (ty, txt, v) {
        $("#Show_CusId").val(v + "-" + txt);
        $("#CusId").val(v);
    });
}
var libtask = {};
libtask.index_setstate = function (taskid, newstate) {
    $.ajax({
        url: '/taskdll/SettaskState',
        data: { taskid: taskid, newstate: newstate },
        type: 'post',
        success: function (data) {
            if (data.code > 0) {
                alert(data.msg);
                location.reload();
            }
            else {
                alert(data.msg);
            }
        }
    });
}

var libcusproject = {};
libcusproject.index_selectcus = function () {
    libjs.showselect('customer', function (ty, txt, v) {
        $("#search_cusid").val(v);
    })
}
libcusproject.index_selectproject = function () {
    libjs.showselect('project', function (ty, txt, v) {
        $("#search_projectid").val(v);
    });
}
libcusproject.index_selectserver = function () {
    libjs.showselect('server', function (ty, txt, v) {
        $("#search_serverid").val(v);
    });
}

var libserverproject = {};
libserverproject.index_selectproject = function () {
    libjs.showselect('project', function (ty, txt, v) {
        $("#search_projectinfo").val(txt);
    });
}
libserverproject.index_selectserver = function () {
    libjs.showselect('server', function (ty, txt, v) {
        $("#search_serverinfo").val(txt);
    });
}

libserverproject.edit_selectserver = function () {
    libjs.showselect('server', function (ty, txt, v) {
        $("#ServerId").val(v);
        $("#show_serverid").text(v + " " + txt);
    });
}

libserverproject.edit_selectproject = function () {
    libjs.showselect('project', function (ty, text, val) {
        $("#ProjectId").val(val);
        $("#show_projectid").text(val + " " + text);
    });
}

var libcmd = {};
libcmd.index_selectcus = function () {
    libjs.showselect('customer', function (ty, txt, v) {
        $("#search_cusid").val(v);
    })
}
libcmd.index_selectproject = function () {
    libjs.showselect('project', function (ty, txt, v) {
        $("#search_projectid").val(v);
    });
}
libcmd.index_selectserver = function () {
    libjs.showselect('server', function (ty, txt, v) {
        $("#search_serverid").val(v);
    });
}

var libworkitem = {};
libworkitem.index_selectmanager1 = function () {
    libjs.showselect('manager', function (ty, txt, v) {
        $("#distributeuserid").val(v);
    })
}
libworkitem.index_selectmanager2 = function () {
    libjs.showselect('manager', function (ty, txt, v) {
        $("#createuserid").val(v);
    });
}
libworkitem.index_delete = function (e) {
    if (confirm("确定要删除吗？")) {
        $.ajax({
            url: '/workitem/delete',
            type: 'post',
            data: { workitemid: $(e).data("id") },
            success: function (data) {
                if (data.code > 0)
                    $(e).parents('tr').remove();
                else {
                    alert(data.msg);
                }
            }
        });
    }
}
libworkitem.detail_exec_click = function () {
    $('#myModal-exec').modal('show');
}
libworkitem.detail_exec = function (e) {
    $.ajax({
        url: '/workitem/DistributeExec',
        type: 'post',
        data: { workitemid: $(e).data("id"), actualtime: $("#exec-actualtime").val(), workRemark: $("#exec-content").val() },
        success: function (data) {
            if (data.code > 0) {
                alert("执行成功！");
                location.href = '/workitem/detail?workitemid=' + $(e).data("id");
            } else {
                alert(data.msg);
            }
        }
    });
}

var libfeedback = {};
libfeedback.index_precheck = function (e) {
    var id = $(e).data('id');
    $("#checktype").val("3");
    $("#btn_check").data("id", id);
    $('#myModal-check').modal('show');
}
libfeedback.edit_selectcus = function () {
    libjs.showselect('customer', function (ty, txt, v) {
        $("#cusId").val(v);
        $("#CusName").val(txt);
        $("#Show_cus").val(v + " " + txt);
    })
}
libfeedback.index_check = function () {
    var feedbackid = $("#btn_check").data("id");
    var remark = $("#check-remark").val();
    var checktype = $("#checktype").val();
    $.ajax({
        url: '/feedback/check',
        type: 'post',
        data: { feedbackid: feedbackid, checktype: checktype, checkremark: remark },
        success: function (data) {
            if (data.code < 0) {
                alert(data.msg);
            }
            else if (data.code == 1) {
                alert("审核成功");
                location.reload();
            } else if (data.code == 2) {
                alert("审核成功");
                location.href = data.data;
                //window.open(data.data, "_blank", "", false);
            }
        }
    });
}


var libcustomer = {};
libcustomer.index_delete = function (e) {
    if (!confirm("确定要删除吗?"))
        return;
    $.ajax({
        url: '/customer/Delete',
        type: 'post',
        data: { cusid: $(e).data('id') },
        success: function (d) {
            if (d.code > 0) {
                $(e).parents('tr').addClass('danger');
                alert('删除成功');
                setTimeout(function () {
                    $(e).parents('tr').hide();
                }, 500);
            } else {
                alert(d.msg);
            }
        }
    })
}

var libmanager = {};
libmanager.index_delete = function (e) {
    if (!confirm("你确定要删除吗?"))
        return;
    $.ajax({
        url: '/manager/DeleteManager',
        type: 'post',
        data: { managerid: $(e).data('id') },
        success: function (d) {
            if (d.code > 0) {
                $(e).parents('tr').addClass('danger');
                alert('删除成功');
                setTimeout(function () {
                    $(e).parents('tr').hide();
                }, 500);
            } else {
                alert(d.msg);
            }
        }
    })
}

var libworkdaily = {};
libworkdaily.index_presubmit = function () {
    var isok = true;
    if (isok && !$("#WorkTime").val()) {
        alert("请选择工作日期!");
        isok = false;
    }
    //if (isok && !$("#Summary").val()) {
    //    alert("请填写概要!");
    //    isok = false;
    //}
    if (isok && !$("#Content").val()) {
        alert("请填写工作内容!");
        isok = false;
    }
    return isok;
}
libworkdaily.index_delete = function (e) {
    if (!confirm("确定要删除吗?"))
        return;
    $.ajax({
        url: '/workdaily/Delete',
        type: 'post',
        data: { workdailyid: $(e).data('id') },
        success: function (d) {
            if (d.code > 0) {
                $(e).parents('tr').addClass('danger');
                alert('删除成功');
                setTimeout(function () {
                    $(e).parents('tr').hide();
                }, 500);
            } else {
                alert(d.msg);
            }
        }
    })
}
libworkdaily.index_datechange = function (e) {
    var date = $("#WorkTime").val();
    var WorkDailyId = $("#WorkDailyId").val();
    if (date) {
        $.ajax({
            url: '/workdaily/checkdate',
            type: 'post',
            data: { date: date, WorkDailyId: WorkDailyId },
            success: function (data) {
                if (data.code > 0) {
                    if (data.data > 0) {
                        $("#date_msg").html('<span class="label label-warning">该日期已存在提交记录</span><a href="/workdaily/edit?workdailyid=' + data.data + '" class="btn btn-sm btn-primary">去编辑</a>');
                    } else {
                        $("#date_msg").html('');
                    }
                }
                else {
                }
            }
        });
    } else {
        $("#date_msg").html('');
    }
}

libworkdaily.index_autobuild = function () {
    var date = $("#WorkTime").val();
    if (!date) {
        alert("请选择日期！");
        return;
    }

    $.ajax({
        url: '/workdaily/BuildDailyFromWork',
        type: 'post',
        data: { date: date },
        success: function (data) {
            if (data.code > 0) {
                $("#Content").val(data.data);
            }
            else {
                alert(data.msg);
            }
        }
    });
}
libworkdaily.index_selectmanager = function () {
    libjs.showselect('manager', function (ty, txt, v) {
        $("#search_managerid").val(v);
    })
}

libworkdaily.report_hasmore = function (e) {
    $.ajax({
        url: '/workdaily/Report',
        type: 'post',
        data: { begintime: $(e).data("begintime"), endtime: $(e).data("endtime"), groupid: $(e).data("groupid") },
        success: function (data) {

            $("#tb_fixheader").remove();
            $("#tb_fixheader_left").remove();

            $("#hasmore_tr").remove();
            $('.cc-workdaily-report table tbody').append(data);

            libworkdaily.report_scroll();
        }
    });
}

libworkdaily.report_scroll = function () {
    $(".cc-workdaily-report").scroll(function (e) {

        var setwidthtop = function () {
            $(".tbreport-main tr:first").children('td').each(function (i, k) {
                console.log($(k).width());
                $($("#tb_fixheader tr td")[i]).width($(this).width());
            });

        }
        var setwidthleft = function () {

            $(".tbreport-main tr").each(function (i, k) {
                $($("#tb_fixheader_left tr")[i]).children('th').width($(k).children('th:first').width());
                $($("#tb_fixheader_left tr")[i]).children('th').height($(k).children('th:first').height());
            });
        }
        var st = $('.cc-workdaily-report').scrollTop();
        if (st >= 0) {
            if ($("#tb_fixheader").length == 0) {
                var thtml = '<table id="tb_fixheader" class="table table-bordered" style="width:' + $(".tbreport-main").width() + 'px;"><tbody><tr class="report-top-head">';
                thtml += $(".cc-workdaily-report table tr:first").html();
                thtml += '</tr></tbody></table>';
                $(".tbreport-main").before(thtml);
            }
            $("#tb_fixheader").show();
            setwidthtop();

        } else {
            $("#tb_fixheader").hide();
        }

        var sl = $('.cc-workdaily-report').scrollLeft();
        if (sl >= 0) {
            if ($("#tb_fixheader_left").length == 0) {
                var thtml = '<table id="tb_fixheader_left" class="table table-bordered" style="width:' + $(".tbreport-main tr:first").children('th:first').attr('width') + ';"><tbody>';
                $(".tbreport-main tr").each(function (i, te) {
                    thtml += '<tr><th class="' + $($(te).children('th')[0]).attr('class') + '">' + $($(te).children('th')[0]).html() + '</th></tr>';
                });
                thtml += '</tbody></table>';
                $(".tbreport-main").before(thtml);
            }
            $("#tb_fixheader_left").show();
            setwidthleft();
        } else {
            $("#tb_fixheader_left").hide();
        }
        $("#tb_fixheader_left").css({
            //   top: st + "px",
            left: sl + 'px'
        });

        $("#tb_fixheader").css({
            top: st + "px"
            //   left: sl + 'px'
        });
        //console.log($('.cc-workdaily-report').scrollTop());
        // console.log(e);
    })
}


//快讯相关
var libfastpost = {};
libfastpost.selectmultycate = function () {
    var curritems = $("#categoryids").val();
    libjs.showselect('postcatelabel', function (items) {
        var strid = "";
        var strname = "";
        $.each(items, function () {
            strid += this.value + ",";
            strname += '<span class="label label-success">' + this.text + '</span>';
        })
        $("#cate_show").html(strname);
        $("#categoryids").val(strid);
    }, null, true, curritems);
}


libjs.pickdatetime = function (ele, ty, op) {
    if (!ele)
        return;
    ty = ty || 'datetime';
    var dateformat = 'yy-mm-dd';
    var timeformat = 'HH:mm:ss';
    switch (ty) {
        case 'date':
            dateformat = 'yy-mm-dd';
            timeformat = '';
            break;
        case 'time':
            dateformat = '';
            timeformat = 'HH:mm:ss';
            break;
        case 'datetime':
            dateformat = 'yy-mm-dd';
            timeformat = 'HH:mm:ss';
            break;
    }
    var currop = { // Default regional settings
        currentText: '现在',
        closeText: '确定',
        amNames: ['AM', 'A'],
        pmNames: ['PM', 'P'],
        timeFormat: timeformat,
        timeSuffix: '',
        timeOnlyTitle: '选择时间',
        timeText: '时间',
        hourText: '时',
        minuteText: '分',
        secondText: '秒',
        millisecText: '毫秒',
        microsecText: '微秒',
        timezoneText: '时区',
        isRTL: false,

        changeMonth: true,
        changeYear: true,
        dateFormat: dateformat
    };
    op = op || {};
    var newop = $.extend(currop, op);
    if (!dateformat && timeformat) {
        $(ele).timepicker(newop);
    }
    if (dateformat && !timeformat) {
        $(ele).datepicker(newop);
    }
    if (dateformat && timeformat) {
        $(ele).datetimepicker(newop);
    }
}

libjs.tips = function (ops) {
    ops = ops || {};
    var thisops = {
        text: '',
        msg: '',
        type: 'alert',
        times: 500,
        callback: null
    };
    $.extend(thisops, ops);
    thisops.text = thisops.text || thisops.msg;
    if (!thisops.text)
        return;
    var tipid = new Date().getTime().toString();
    var temp = '<div class="cc-toptips" id="tip_{tipid}" style="display:none;"><div class="alert alert-{type}">{msg}</div></div>';
    var thtml = temp.replace("{type}", thisops.type).replace("{msg}", thisops.text).replace("{tipid}", tipid);
    $(document.body).append(thtml);
    $("#tip_" + tipid).slideDown(200, function () {
        setTimeout(function () {
            $("#tip_" + tipid).fadeOut(100, function () {
                $("#tip_" + tipid).remove();
                if (thisops.callback) {
                    thisops.callback();
                }
            });
        }, thisops.times);
    })
}

libjs.articlecollection = {};
libjs.articlecollection.setmark = function (id, markid, remarkmsg, cb2) {
    $("#collection_setmark").remove();
    var t = ' <form id="collection_setmark">    ' +
                '<div class="form-group">' +
                '   <label>类型:</label><input type="hidden" id="articleid_hide" /> ' +
                  '<span class="mark_con"><span class="articlemark articlemark0" data-markid="0"></span></span>' +
                  '<span class="mark_con"><span class="articlemark articlemark1" data-markid="1"></span></span>' +
                  '<span class="mark_con"><span class="articlemark articlemark2" data-markid="2"></span></span>' +
                  '<span class="mark_con"><span class="articlemark articlemark3" data-markid="3"></span></span>' +
                  '<span class="mark_con"><span class="articlemark articlemark4" data-markid="4"></span></span>' +
                  '<span class="mark_con"><span class="articlemark articlemark5" data-markid="5"></span></span>' +
               '</div><div class="form-group">   <label>备注: </label> ' +
               '<textarea class="form-control cc-display-inline-block" rows="3" style="width:100%;" name="remark"></textarea> ' +
           '</div>  </form>';
    libjs.showdialog("设置标记", t, '保存', function (did) {
        var tid = $("#collection_setmark #articleid_hide").val();
        var tmark = $("#collection_setmark .articlemark.check").data("markid");
        var tmsg = $("#collection_setmark textarea").val();
        libjs.articlecollection.saveflag(tid, tmark, tmsg, function () {
            $("#articlemark_id_" + tid).removeClass("articlemark" + markid).addClass("articlemark" + tmark).attr("title", tmsg);
            libjs.closedialog(did);
            if (cb2) cb2(tid, tmark, tmsg);
        })
    });
    $("#collection_setmark .articlemark").click(function () {
        $("#collection_setmark .articlemark").removeClass("check").html('');
        $(this).addClass("check").html('√已选');
    });
    markid = markid || '0';
    $("#collection_setmark #articleid_hide").val(id);
    $("#collection_setmark .articlemark" + markid).addClass("check").html('√已选');
    $("#collection_setmark textarea").val(remarkmsg);
}

libjs.articlecollection.saveflag = function (id, flag, remark, cb) {
    $.ajax({
        url: '/post/ArticleCollection/SaveFlag',
        type: 'post',
        data: { id: id, remark: remark, flag: flag },
        success: function (data) {
            if (data.code > 0) {
                alert("保存成功！");
                if (cb) cb();
            } else {
                alert(data.msg);
            }
        },
        error: function () {
            alert('网络出错！');
        }
    });
};

libjs.cmds = {};
libjs.cmds.buildcmd = function (cmdtype, taskid, nodeid, cb) {
    $.ajax({
        url: '/cmd/buildcmd',
        type: 'post',
        data: { cmdtype: cmdtype, taskid: taskid, nodeid: nodeid },
        success: function (data) {
            if (cb)
                cb(data);
        },
        error: function () {
            if (cb)
                cb({ code: -1, msg: '网络出错' });
        }
    });
}

libjs.selectnode = function (tg) {
    libjs.showselect('node', function (ty, te, va) {
        $(tg).val(va);
    });
}

libjs.selecttask = function (tg) {
    libjs.showselect('task', function (ty, te, va) {
        $(tg).val(va);
    });
}