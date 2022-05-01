
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class XJ_StatusMachine {
    [Serializable]
    public class Parameter {
        public string name;//变量名
        public int value;//变量值
    }
    [Serializable]
    public class Translation {
        public XJ_StringSelector_Single currStatus;//当前状态
        public XJ_StringSelector_Single nextStatus;//下一状态
        public XJ_StringSelector_Single paraLeft;//变量(左)
        public XJ_StringSelector_Single relationOperator;//关系运算符(6种，分别是<、>、=、<=、>=、!=
        public XJ_StringSelector_Single paraRight;//变量(右)
    }


    [SerializeField]
    private List<string> status;//状态列表
    [SerializeField]
    private List<Parameter> parameter;//变量列表
    [SerializeField]
    private List<Translation> translation;//状态转换
    [SerializeField]
    private XJ_StringSelector_Single currStatus;//当前状态
    [SerializeField]
    private bool valueChange;//当使用inspector修改了变量列表parameter时该值置为真，以便能够判断是否去更新数据
    private bool firstRun = true;//判断是否初次运行。当该值为false时会去刷新索引值

    [SerializeField]
    private List<string> __relationOperator;//关系运算符（给XJ_StringSelector使用）
    [SerializeField]
    private List<string> __paraList;//变量（给XJ_StringSelector使用）
    [SerializeField]
    private List<bool> __fold;//折叠（给Editor使用。麻烦的东西）

    private Dictionary<string, int> paraDict_value = new Dictionary<string, int>();//变量映射。设置它的目的自然是为了提高查找效率，记录着变量名对应的值
    private Dictionary<string, Dictionary<string, List<int>>> transDict_index = new Dictionary<string, Dictionary<string, List<int>>>();//状态映射。记录了对应着trnaslations的索引/下标，也是为了提高查找效率
    private Dictionary<string, int> paraDict_index = new Dictionary<string, int>();//用于判断变量名是否存在



    public string GetStatus() {//获取当前状态
        return currStatus.selectedStr;
    }
    public int GetParameter(string name) {//获取变量。如果变量不存在将直接报错
        if (paraDict_index.ContainsKey(name) == false)
            throw new Exception("变量【" + name + "】不存在！");
        return paraDict_value[name];
    }
    public HashSet<string> GetParameterSet() {//获取变量名的集合(不建议也没必要去调用，主要用于debug输出
        return new HashSet<string>(paraDict_index.Keys);
    }
    public HashSet<string> GetStatusSet() {//获取状态名的集合(不建议也没必要去调用，主要用于debug输出
        return new HashSet<string>(transDict_index.Keys);
    }
    public void SetParameter(string name, int value) {//设置变量。如果变量不存在那么将直接报错
        if (paraDict_index.ContainsKey(name) == false)
            throw new Exception("变量【" + name + "】不存在！");
        parameter[paraDict_index[name]].value = value;
        paraDict_value[name] = value;
    }
    public void SetCurrStatus(string name) {//设置当前状态。如果状态不存在将直接报错
        if (transDict_index.ContainsKey(name) == false)
            throw new Exception("状态【" + name + "】不存在！");
        currStatus.selectedStr = name;
    }


    public void UpdateStatus() {//刷新状态
        UpdateDict();
        if (transDict_index.TryGetValue(currStatus.selectedStr, out Dictionary<string, List<int>> dict)) {//获取当前状态对应的转换映射dict
            foreach (var item in dict) {
                bool success = true;
                foreach (var index in item.Value) {
                    var trans = translation[index];
                    var left = paraDict_value[trans.paraLeft.selectedStr];//左值
                    var right = paraDict_value[trans.paraRight.selectedStr];//右值
                    var oper = trans.relationOperator.selectedStr;//关系符
                    if (oper.Length == 0) {
                        success = false;
                        break;
                    }
                    {
                        bool small = left < right;//小于
                        bool equal = left == right;//等于
                        if (oper.Contains("<") && small)
                            continue;
                        if (oper.Contains(">") && (small == false) && (equal == false))
                            continue;
                        if (oper.Contains("=") && (oper.Contains("!") == false) && equal)
                            continue;
                        success = false;
                        break;
                    }
                }
                if (success) {
                    currStatus.selectedStr = item.Key;
                    break;
                }
            }
        }
    }
    private void UpdateDict() {//刷新索引
        if (valueChange|| firstRun) {
            valueChange = false;
            firstRun = false;
            paraDict_value.Clear();
            transDict_index.Clear();
            paraDict_index.Clear();
            var pst = 0;
            foreach (var p in parameter) {
                paraDict_value.Add(p.name, p.value);
                paraDict_index.Add(p.name, pst);
                ++pst;
            }
            pst = 0;
            foreach (var t in translation) {
                var str_curr = t.currStatus.selectedStr;
                var str_next = t.nextStatus.selectedStr;
                if (str_next.Length == 0)
                    continue;

                if (transDict_index.TryGetValue(str_curr, out Dictionary<string, List<int>> dict)) {
                    if(dict.TryGetValue(str_next, out List<int> lst)) {
                        lst.Add(pst);
                    }
                    else {
                        lst = new List<int> { pst };
                        dict.Add(str_next, lst);
                    }
                }
                else {
                    dict = new Dictionary<string, List<int>>();
                    dict.Add(str_next, new List<int> { pst });
                    transDict_index.Add(str_curr, dict);
                }
                ++pst;
            }
        }
    }
}
