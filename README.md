# MarkingMessageBox
WPF带淡入淡出动画遮罩消息框

# 使用说明：
本代码为一个Window窗口代码，将此代码直接添加进项目使用即可

# 代码调用规则：
原型：
    MarkingMessageBox.Instance.Invoke(string msg, string caption, Window owner = null)
    
调用：
    (1)无遮罩动画版：MarkingMessageBox.Instance.Invoke("your message", "your caption"); 
    (2)有遮罩动画版：MarkingMessageBox.Instance.Invoke("your message", "your caption", your parent's window);
