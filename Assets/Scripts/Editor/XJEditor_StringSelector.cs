using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using UnityEditorInternal;


class XJEditor_StringSelector {
    //  private static SerializedProperty Search(SerializedProperty property, string prefix, string labelText) {//搜寻带有prefix前缀的数据，或者以labelText为名的数据
    //      var path = property.propertyPath.Split('.');//路径
    //      var obj = property.serializedObject;
    //      var len = path.Length;
    //      if (path[len - 1].Contains("["))
    //          --len;
    //      for (var pst = len; pst-- > 0;) {
    //          SerializedProperty rst = null;
    //          for (var p = 0; p < pst; ++p)
    //              rst = rst == null ? obj.FindProperty(path[p]) : rst.FindPropertyRelative(path[p]);
    //          var rst1 = rst == null ? obj.FindProperty(labelText) : rst.FindPropertyRelative(labelText);
    //          if (rst1 != null) //搜寻成功
    //              return rst1;
    //
    //          string name = prefix;
    //          for (var p = pst; p < len; ++p)
    //              name = name + "_" + path[p];
    //          var rst2 = rst == null ? obj.FindProperty(name) : rst.FindPropertyRelative(name);
    //          if (rst2 != null) //搜寻成功
    //              return rst2;
    //      }
    //      return null;
    //  }

    public static void OnGUI(Rect position, SerializedProperty property, GUIContent label, bool multiSelected) {
        SerializedProperty s_selected = property.FindPropertyRelative("selectedStr");//寻找选中的字串
        SerializedProperty s_list = XJEditor_StringSelector.Search(property, label.text);//根据标签label寻找列表数据（反正标签也没啥用还不如拿来搜寻数据

        if ((s_selected != null) && multiSelected)
            s_selected = s_selected.FindPropertyRelative("Array");

        Dictionary<string, string> displayItem = XJEditor_StringSelector.Get_DisplayItem(s_list);//列表显示的字串
        HashSet<string> selectedItem = XJEditor_StringSelector.Get_SelectedItem(s_selected, displayItem, multiSelected);//选中的字串
        string hint = XJEditor_StringSelector.Get_Hint(selectedItem);//按钮显示的文本

        if (label == GUIContent.none)
            label = new GUIContent(" ");//让它不为空，便于对齐
        EditorGUI.BeginProperty(position, label, property);
        if (EditorGUI.DropdownButton(position, new GUIContent(hint), FocusType.Passive, "Popup")) {
            GenericMenu menu = new GenericMenu();
            foreach (var item in displayItem) {
                var str = item.Key;
                var extra = item.Value;
                if (str.Length == 0)
                    continue;
                menu.AddItem(new GUIContent(str + extra), selectedItem.Contains(str), XJEditor_StringSelector.Click, new List<object> { s_selected, str, selectedItem.Contains(str), multiSelected });
            }
            menu.ShowAsContext();
            menu.DropDown(position);
        }
        EditorGUI.EndProperty();
    }

    public static SerializedProperty Search(SerializedProperty property, string labelText) {//搜寻以labelText为名的数据
        var path = property.propertyPath.Split('.');//路径
        var obj = property.serializedObject;
        var len = path.Length;
        if (path[len - 1].Contains("]"))
            --len;
        for (var pst = len; pst-- > 0;) {
            SerializedProperty rst = null;
            for (var p = 0; p < pst; ++p)
                rst = rst == null ? obj.FindProperty(path[p]) : rst.FindPropertyRelative(path[p]);
            rst = rst == null ? obj.FindProperty(labelText) : rst.FindPropertyRelative(labelText);
            if (rst != null) //搜寻成功
                return rst;
        }
        return null;
    }
    public static Dictionary<string, string> Get_DisplayItem(SerializedProperty list) {//返回展示菜单显示的字串
        Dictionary<string, string> showList = new Dictionary<string, string>();
        if ((list != null) && (list.isArray == true)) {
            var dict = new Dictionary<string, int>();
            for (int pst = 0, size = list.arraySize; pst < size; ++pst) {
                var str = list.GetArrayElementAtIndex(pst).stringValue;
                if (str.Replace(" ","").Length == 0)//如果字串为空那么就pass
                    continue;
                if (dict.TryGetValue(str, out int num))
                    dict[str] = num + 1;
                else
                    dict.Add(str, 1);
            }
            foreach (var item in dict)
                showList.Add(item.Key, (item.Value == 1 ? "" : System.String.Format("  ({0})", item.Value.ToString())));
        }
        return showList;
    }
    public static HashSet<string> Get_SelectedItem(SerializedProperty selected, Dictionary<string, string> displayItem, bool multiSelected) {//返回选中的字串，传入的displayItem恰好为Get_DisplayItem返回的数据
        HashSet<string> set = new HashSet<string>();//选中的字串
        if (selected != null) {
            //            if (selected.isArray) {//你牛，你伟大，nmsl，selected是数组还是字串都判断不出来还得让我额外用个变量multiSelected来记录，真有你的，臭垃圾
            if (multiSelected) {
                for (int pst = 0, size = selected.arraySize; pst < size; ++pst) {
                    var str = selected.GetArrayElementAtIndex(pst).stringValue;
                    if (displayItem.ContainsKey(str))
                        set.Add(str);
                    else {//清除那些在列表中不存在的字串
                        selected.DeleteArrayElementAtIndex(pst);
                        --pst;
                        --size;
                    }
                }
            }
            else {
                if (displayItem.ContainsKey(selected.stringValue))
                    set.Add(selected.stringValue);
                else
                    selected.stringValue = "";//不存在字串那么就赋空值
            }
        }

        return set;
    }


    public static string Get_Hint(HashSet<string> selectedItem) {//获取按钮显示的文本
        if (selectedItem.Count == 0)
            return "<未选择>";
        else if (selectedItem.Count != 1)
            return "<选中多个>";
        var iter = selectedItem.GetEnumerator();
        iter.MoveNext();
        return iter.Current;
    }
    public static void Click(object list) {//list里头的元素依次是：列表数据(SerializedProperty)、字串(string)、字串是否选中(bool)、是否多选(bool)
        var lst = (List<object>)list;
        var selected = (SerializedProperty)lst[0];//列表数据
        var str = (string)lst[1];//字串
        var tick = (bool)lst[2];//当前字串是否选中
        var multiSelected = (bool)lst[3];//判断是否多选

        if (selected != null) {
            //            if (selected.isArray) { //也是很伟大了呢，逼我用更多本不必要使用的数据来记录数据类型
            if (multiSelected) {//多选
                if (tick) {//清除选中的字串
                    for (var pst = selected.arraySize; pst-- > 0;) {
                        if (selected.GetArrayElementAtIndex(pst).stringValue == str) {
                            selected.DeleteArrayElementAtIndex(pst);
                            break;
                        }
                    }
                }
                else {//加入选中的字串
                    selected.InsertArrayElementAtIndex(0);
                    selected.GetArrayElementAtIndex(0).stringValue = str;
                }
            }
            else //单选
                selected.stringValue = str;
            selected.serializedObject.ApplyModifiedProperties();//应用修改
        }
    }
}


[CustomPropertyDrawer(typeof(XJ_StringSelector_Single))]
class XJEditor_StringSelector_Single : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        XJEditor_StringSelector.OnGUI(position, property, label, false);
    }
}




[CustomPropertyDrawer(typeof(XJ_StringSelector_Multi))]
class XJEditor_StringSelector_Multi : PropertyDrawer {
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        XJEditor_StringSelector.OnGUI(position, property, label, true);
    }
}
