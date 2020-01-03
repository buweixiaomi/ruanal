
var addCofigDiaConfig;
function showConfigDialog(config, role, callbk,addiconfig) {
    config = config || '{}';
    config = JSON.parse(config);
    var nodeconfig = new configNode(role, addiconfig);
    for (var a in config) {
        nodeconfig.setConfig(a, config[a]);
    }
    var parseToHtml = function (configs) {
        var tmp = '<div class="config-item">' +
                            '<input type="text" name="{@key}" data-type="{@type}" class="form-control cc-block cc-width-1-2 configkey" {@readonly} value="{@title}">：' +
                            '<input type="text" id="{@id}" class="form-control cc-block cc-width-3-5 configvalue" value="{@value}" placeholder="{@desc}">{@phtml}{@removebtn}' +
                     '</div>';
        var removebtn = '<button class="btn btn-danger configdel" type="button" onclick="$(this).parent().remove();">删除</button>';
        var addbtn = '<div><button class="btn btn-primary configdel" type="button" onclick="addCofigDiaConfig()">添加</button></div>';
        addCofigDiaConfig = function () {
            var item = tmp.replace(/\{@id}/g, '')
          .replace(/\{@key}/g, '')
          .replace(/\{@readonly}/g, '')
          .replace(/\{@title}/g, '')
          .replace(/\{@value}/g, '')
          .replace(/\{@type}/g, 'user')
          .replace(/\{@phtml}/g, '')
          .replace(/\{@removebtn}/g, removebtn)
            $(".diaconfig").append(item);
        }
        var items = [];
        for (var i = 0; i < configs.length; i++) {
            var item = tmp.replace(/\{@id}/g, configs[i].id||'')
            .replace(/\{@key}/g, configs[i].key)
            .replace(/\{@readonly}/g, configs[i].type == 'sys' ? 'readonly' : '')
            .replace(/\{@title}/g, configs[i].title)
                .replace(/\{@value}/g, configs[i].value)
                .replace(/\{@type}/g, configs[i].type)
                .replace(/\{@desc}/g, configs[i].desc||'')
          .replace(/\{@phtml}/g, configs[i].phtml||'')
            .replace(/\{@removebtn}/g, configs[i].type == 'sys' ? '' : removebtn)
            items.push(item);
        }
        var ahtml = '<div class="diaconfig">' + items.join(" \r\n") + '</div>' + addbtn;
        return ahtml;
    }
    var ahtml = parseToHtml(nodeconfig.getAll());
    libjs.showdialog('编辑配置', ahtml, '确定', function (did) {
        var configobj = {};
        $(".diaconfig .config-item").each(function (i, e) {
            var vt = $(e).find("input.configkey").data('type');
            var k = vt == 'sys' ? $(e).find("input.configkey").attr('name') :
                $(e).find("input.configkey").val();
            var v = $(e).find("input.configvalue").val();
            if (!k)
                return;
            configobj[k] = v || '';
        });
        if (callbk) {
            callbk(JSON.stringify(configobj, null, '\t'));
        }
        libjs.closedialog(did);
    });
}

function config_editor_selectqy(tar) {
    var value = $(tar).val();
    libjs.showselect('enterprise', function (vals) {
        var v = [];
        for (var a = 0; a < vals.length; a++) {
            v.push(vals[a].value);
        }
        $(tar).val(v.join(','));
    }, null, true, value, true);
}

function configNode(role, addiconfig) {
    var getsyskey = function (role) {
        var DBConnKey = "_BusinessDBConn";
        var Dis_SpecifyEntsKey = "_Dis_SpecifyEnts";
        var Dis_ExceptEntsKey = "_Dis_ExceptEnts";
        var Dis_InstanceCountKey = "_Dis_InstanceCount";
        //  var Dis_SpecifyEntShopsKey = "_Dis_SpecifyEntShops";
        //   var Dis_ExceptEntShopsKey = "_Dis_ExceptEntShops";
        var Dis_ContainDBInfoKey = "_Dis_ContainDBInfo";
        var Dis_NoRunKeyReg = "_NoRunKeyReg";
        var Dis_OnlyRunKeyReg = "_OnlyRunKeyReg";
       // var Dis_ContainShopInfoKey = "_Dis_ContainShopInfo";
        var configs = [];
        configs.push({
            key: DBConnKey,
            roles: ["node", 'task'],
            title: '业务主库连接',
            type: 'sys',
            value: '',
            phtml: '',
            id:''
        });
        configs.push({
            key: Dis_InstanceCountKey,
            roles: ["node", 'task'],
            title: '任务并行数',
            type: 'sys',
            value: '',
            phtml: '',
            id: ''
        });
        configs.push({
            key: Dis_SpecifyEntsKey,
            roles: ['task'],
            title: '指定企业',
            type: 'sys',
            value: '',
            phtml: '<button type="button" class="btn btn-default" onclick="config_editor_selectqy(\'#__dis_sp_enter\')">...</button>',
            id: '__dis_sp_enter'
        }); configs.push({
            key: Dis_ExceptEntsKey,
            roles: ['task'],
            title: '排除企业',
            type: 'sys',
            value: '',
            phtml: '<button type="button" class="btn btn-default" onclick="config_editor_selectqy(\'#__dis_ex_enter\')">...</button>',
            id: '__dis_ex_enter'
        });
        //configs.push({
        //    key: this.Dis_SpecifyEntShopsKey,
        //    roles: ['task'],
        //    title: '指定店铺',
        //    type: 'sys',
        //    value: ''
        //}); configs.push({
        //    key: this.Dis_ExceptEntShopsKey,
        //    roles: ['task'],
        //    title: '排除店铺',
        //    type: 'sys',
        //    value: ''
        //});
        configs.push({
            key: Dis_ContainDBInfoKey,
            roles: ['task'],
            title: '包含企业库信息',
            type: 'sys',
            value: 'true',
            phtml: '',
            id: ''
        }); 
        configs.push({
            key: Dis_NoRunKeyReg,
            roles: ['node', 'task'],
            title: '排除RunKey(正则,多个||隔开)',
            type: 'sys',
            value: '',
            phtml: '',
            id: ''
        });

        configs.push({
            key: Dis_OnlyRunKeyReg,
            roles: ['node', 'task'],
            title: '仅包含RunKey(正则,多个||隔开)',
            type: 'sys',
            value: '',
            phtml: '',
            id: ''
        });
        //configs.push({
        //    key: Dis_ContainShopInfoKey,
        //    roles: ['task'],
        //    title: '包含店铺信息',
        //    type: 'sys',
        //    value: 'true'
        //});
        var resultconfig = [];
        for (var k = 0; k < configs.length; k++) {
            if (configs[k].roles.indexOf(role) >= 0) {
                resultconfig.push(configs[k]);
            }
        }
        if (addiconfig) {
            for (var k = 0; k < addiconfig.length; k++) {
                resultconfig.push({
                    key: addiconfig[k].key,
                    roles: [],
                    title: addiconfig[k].title,
                    type: 'sys',
                    value: '',
                    desc: addiconfig[k].desc,
                    phtml: '',
                    id: ''
                });
            }
        }

        return resultconfig;
    }

    this.configs = getsyskey(role);
}

configNode.prototype.setConfig = function (key, val) {
    var _self = this;

    var exist = false;
    for (var i = 0; i < _self.configs.length; i++) {
        if (_self.configs[i].key == key) {
            _self.configs[i].value = val;
            exist = true;
        }
    }
    if (!exist) {
        _self.configs.push({
            key: key,
            title: key,
            roles: [],
            type: 'user',
            value: val
        });
    }
}
configNode.prototype.getAll = function () {
    return this.configs;
}
