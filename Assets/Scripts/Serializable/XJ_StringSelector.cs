using System.Collections.Generic;
using UnityEngine;


//该组件直接使用不会生效，要同时在所属的类的Editor进行改写(也就是自定义Inspector)

[System.Serializable]//可序列化
public class XJ_StringSelector_Single {//字串选择器
    public string selectedStr;//被选中的字串（未选中时该值为""
}


[System.Serializable]//可序列化
public class XJ_StringSelector_Multi {//字串选择器
    public List<string> selectedStr;//被选中的字串
}

