using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditorInternal;




[CustomPropertyDrawer(typeof(XJ_StatusMachine.Parameter))]
class XJEditor_StatusMachine_Parameter : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        var path = property.propertyPath;
        if (path[path.Length - 1] == ']')//如果是作为数组的元素的话那么将label移除
            label = GUIContent.none;
        else if (label == GUIContent.none)
            label = new GUIContent(" ");//让它不为空，便于对齐

        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);//显示字段标签，返回标签占用后剩余的区域
        var width = (position.width - 20) / 2;
        position.width = width;


        var name = property.FindPropertyRelative("name");
        var value = property.FindPropertyRelative("value");
        name.stringValue = EditorGUI.TextField(position, name.stringValue);
        position.x += width;
        EditorGUI.LabelField(position, " =");
        position.x += 20;
        value.intValue = EditorGUI.IntField(position, value.intValue);

        EditorGUI.EndProperty();
    }
}



[CustomPropertyDrawer(typeof(XJ_StatusMachine.Translation))]
class XJEditor_StatusMachine_Translation : PropertyDrawer {
    private class Info {
        public bool isLabel;//判断信息是否用于label
        public string name;//如果是label那么就为标签名，否则就是Selector的属性名，将传进property.FindPropertyRelative里头
        public string selectorLabel;//给Selector传进标签名。特殊的数据传递方式
        public float offset;//偏移量
        public Info(bool Label, string Name, float Offset, string SelectorLabel = "") {
            name = Name;
            isLabel = Label;
            offset = Offset;
            selectorLabel = SelectorLabel;
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        var path = property.propertyPath;
        if (path[path.Length - 1] == ']')//如果是作为数组的元素的话那么将label移除
            label = GUIContent.none;
        else if (label == GUIContent.none)
            label = new GUIContent(" ");//让它不为空，便于对齐

        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);//显示字段标签，返回标签占用后剩余的区域

        var widthSelector = position.width;
        var widthLabel = 20f;
        if (widthSelector < widthLabel * 13) { //宽度不足
            widthSelector = (widthLabel = widthSelector / 13) * 2;
        }
        else {//宽度管够
            widthSelector = (widthSelector - 5 * widthLabel) / 4;
        }


        EditorGUI.BeginProperty(position, label, property);
        var lst = new List<Info> {
            new Info(false,"currStatus",widthSelector,"status"),
            new Info(true,"=>",widthLabel),
            new Info(false,"nextStatus",widthSelector,"status"),
            new Info(true," ",2*widthLabel),
            new Info(false,"paraLeft",widthSelector,"__paraList"),
            new Info(false,"relationOperator",2*widthLabel,"__relationOperator"),
            new Info(false,"paraRight",widthSelector,"__paraList"),
        };
        foreach (var info in lst) {
            position.width = info.offset;
            if (info.isLabel)
                EditorGUI.LabelField(position, info.name);
            else {
                var selector = property.FindPropertyRelative(info.name);
                EditorGUI.PropertyField(position, selector, new GUIContent(info.selectorLabel));
            }
            position.x += info.offset;
        }
        EditorGUI.EndProperty();

    }
}



[CustomPropertyDrawer(typeof(XJ_StatusMachine))]
class XJEditor_StatusMachine : PropertyDrawer {
    private class Data {
        public ReorderableList status;
        public ReorderableList parameter;
        public ReorderableList translation;
        public SerializedProperty paraList;
        public SerializedProperty fold;
        public SerializedProperty valueChange;
        public SerializedProperty currStatus;
        public float propertyHeight;//组件高度
    }
    private SerializedProperty sp;//减少压力，每次调用OnGUI时把property传进这里
    private Dictionary<string, Data> dataDict = new Dictionary<string, Data>();//数据，string对应着property.propertyPath


    private Data InitData(SerializedProperty property) {
        sp = property;
        if (dataDict.TryGetValue(sp.propertyPath, out Data data) == false) {
            data = new Data();
            dataDict[sp.propertyPath] = data;

            data.status = GetList("status", "状态  (字符串)");
            data.parameter = GetList("parameter", "变量  (左字符串 右整型数)");
            data.translation = GetList("translation", "转换表  (左状态转换 右转换条件)");
            data.valueChange = sp.FindPropertyRelative("valueChange");
            data.currStatus = sp.FindPropertyRelative("currStatus");
            data.paraList = sp.FindPropertyRelative("__paraList").FindPropertyRelative("Array");
            data.fold = sp.FindPropertyRelative("__fold").FindPropertyRelative("Array");
            data.propertyHeight = 500;
            {
                while (data.fold.arraySize < 4)
                    data.fold.InsertArrayElementAtIndex(0);
                var oper = sp.FindPropertyRelative("__relationOperator");
                if (oper.arraySize == 0) {
                    foreach (var p in new List<string> { "<", ">", "=", "<=", ">=", "!=" }) {
                        oper.InsertArrayElementAtIndex(0);
                        oper.GetArrayElementAtIndex(0).stringValue = p;
                    }
                }
            }
        }
        data.valueChange.boolValue = true;
        return data;
    }
    private void UpdateParaList() {//将parameter的数据同步到paraList中
        var d = dataDict[sp.propertyPath];
        var para = d.parameter.serializedProperty;//.FindPropertyRelative("Array");
        var list = d.paraList;
        var lenP = para.arraySize;
        var lenL = list.arraySize;
        var pst = 0;

        for (; pst < lenP && pst < lenL; ++pst)
            list.GetArrayElementAtIndex(pst).stringValue = para.GetArrayElementAtIndex(pst).FindPropertyRelative("name").stringValue;//直接赋值覆盖，不做无意义的判断(反正就是个字符串而已，复制的代价要小的多
        var diff = lenP - lenL;
        if (diff > 0) {//说明有新数据
            while (diff-- > 0)
                list.InsertArrayElementAtIndex(pst);
            for (; pst < lenP; ++pst)
                list.GetArrayElementAtIndex(pst).stringValue = para.GetArrayElementAtIndex(pst).FindPropertyRelative("name").stringValue;
        }
        else if (diff < 0) {//说明删除了数据
            while (diff++ < 0)
                list.DeleteArrayElementAtIndex(pst);
        }
    }
    private ReorderableList GetList(string name, string hint) {
        var list = new ReorderableList(sp.serializedObject, sp.FindPropertyRelative(name));
        list.drawHeaderCallback = (Rect rect) => {//绘制表头
            EditorGUI.LabelField(rect, hint);
        };
        list.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {//绘制数据
            var element = list.serializedProperty.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(rect, element, GUIContent.none);
        };
        return list;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        var data = InitData(property);
        UpdateParaList();

        System.Func<int, string, string, bool> FolderField = (int pst, string hint1, string hint2) => {
            var b = data.fold.GetArrayElementAtIndex(pst);
            var w = position.width;
            //            position.width = System.Math.Max(100, w / 3);
            b.boolValue = EditorGUI.Foldout(position, b.boolValue, !b.boolValue ? hint1 : hint2, true);
            position.width = w;
            return b.boolValue;
        };


        var offsetX = 20;
        var offsetY = 20;
        position.height = offsetY;
        EditorGUI.BeginProperty(position, label, property);
        data.propertyHeight = position.y;
        if (FolderField(0, "状态机", "状态机")) {
            position.x += offsetX;
            position.width -= offsetX;
            position.y += offsetY;

            var x = position.x;
            var width = position.width;
            {
                EditorGUI.LabelField(position, "当前状态：");
                position.x += System.Math.Min(width / 2, 60);
                position.width = System.Math.Min(width / 2, 100);
                EditorGUI.PropertyField(position, data.currStatus, new GUIContent("status"));
                position.x = x;
                position.width = width;
                position.y += offsetY;
            }

            {
                float maxH = 0;
                position.width = (width - 20) / 2;
                if (FolderField(2, "状态", "")) {
                    data.status.DoList(position);
                    var height = data.status.GetHeight();
                    maxH = height;
                }
                position.x += width / 2 + 10;
                if (FolderField(1, "变量", "")) {
                    data.parameter.DoList(position);
                    var height = data.parameter.GetHeight();
                    if (maxH < height)
                        maxH = height;
                }
                position.x = x;
                position.width = width;
                position.y += offsetY;
                position.y += maxH;
            }
            if (FolderField(3, "转换表", "")) {
                data.translation.DoList(position);
                var height = data.translation.GetHeight();
                position.y += height;
            }
        }
        data.propertyHeight = position.y - data.propertyHeight+20;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        if (dataDict.TryGetValue(property.propertyPath, out Data data))
            return data.propertyHeight;
        return base.GetPropertyHeight(property, label) * 50;
    }
}



