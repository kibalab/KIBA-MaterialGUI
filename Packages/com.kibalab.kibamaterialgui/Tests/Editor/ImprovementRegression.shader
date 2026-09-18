Shader "Review/MaterialGUI"
{
    Properties
    {
        [HideInInspector] _Hidden ("Hidden", Float) = 0
        [Group(Conditional)][ShowIf(_Hidden, 1)] _Dependent ("Dependent", Float) = 0
        [Group(Data)][GradientTexture] _Gradient ("Gradient", 2D) = "white" {}
        [Group(Data,More,Fields)] _Integer ("Integer", Integer) = 2
        [MinMaxSlider(n1, 1)] _Limits ("Limits", Vector) = (0,1,0,0)
    }
    SubShader { Pass {} }
}
