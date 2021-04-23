// Decompiled with JetBrains decompiler
// Type: FrooxEngine.LogiX.LogixNodeSelector2
// Assembly: FrooxEngine, Version=2020.9.23.1035, Culture=neutral, PublicKeyToken=null
// MVID: E6EAC99A-2316-4380-9AFF-8572E63C6EE6
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\NeosVR\Neos_Data\Managed\FrooxEngine.dll

using BaseX;
using FrooxEngine.UIX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FrooxEngine.LogiX.Data;
using FrooxEngine.LogiX.String;
using FrooxEngine.LogiX.Cast;
using FrooxEngine.LogiX.WorldModel;
using FrooxEngine.LogiX.ProgramFlow;
using FrooxEngine.LogiX.Operators;

namespace FrooxEngine.LogiX
{
    [OldNamespace("FrooxEngine")]
    public class LogixNodeSelector2 : NeosSwapCanvasPanel
    {
        public const float DOUBLE_CLICK_INTERVAL = 0.35f;
        public readonly Sync<bool> ShowExperimental;
        protected readonly Sync<string> _currentPath;

        protected override void OnAttach()
        {
            this.Slot.AttachComponent<SearchBlock>(true, (Action<SearchBlock>)null);
            base.OnAttach();
            this.CanvasSize = new float2(700f, 700f);
            this.CanvasScale = 0.0005714286f;
            this.Panel.AddCloseButton();
            this.Panel.AddParentButton();
            this.Panel.Title = "LogiX Nodes";
            this.BuildUI("LogiX", false);
        }

        private void BuildUI(string path, bool genericType = false)
        {
            UIBuilder uiBuilder1 = this.SwapPanel(this._currentPath.Value == null ? NeosSwapCanvasPanel.Slide.None : (this._currentPath.Value.Length > path.Length ? NeosSwapCanvasPanel.Slide.Right : NeosSwapCanvasPanel.Slide.Left), 0.25f);
            uiBuilder1.ScrollArea(new Alignment?());
            UIBuilder uiBuilder2 = uiBuilder1;
            float2 one1 = float2.One;
            float2 float2_1 = one1 * 96;
            ref float2 local1 = ref float2_1;
            float2 one2 = float2.One;
            float2 float2_2 = one2 * 4;
            ref float2 local2 = ref float2_2;
            uiBuilder2.GridLayout(in local1, in local2, Alignment.TopLeft);
            uiBuilder1.FitContent(SizeFit.Disabled, SizeFit.PreferredSize);
            this._currentPath.Value = path;
            List<LogixNodeSelector2.ElementListing> list = Pool.BorrowList<LogixNodeSelector2.ElementListing>();
            if (!genericType)
            {
                CategoryNode<System.Type> subcategory1 = WorkerInitializer.ComponentLibrary.GetSubcategory(path);
                if (!(subcategory1.Name == "LogiX"))
                {
                    UIBuilder uiBuilder3 = uiBuilder1;
                    LocaleString text = (LocaleString)"(back)";
                    color color = new color(1f, 0.8f, 0.8f, 1f);
                    ref color local3 = ref color;
                    ButtonEventHandler<string> callback = new ButtonEventHandler<string>(this.OnOpenCategoryPressed);
                    string path1 = subcategory1.Parent.GetPath();
                    uiBuilder3.Button<string>(text, in local3, callback, path1, 0.35f);
                }
                foreach (CategoryNode<System.Type> subcategory2 in subcategory1.Subcategories)
                {
                    if (!(subcategory2.Name == "Experimental") || this.ShowExperimental.Value)
                    {
                        UIBuilder uiBuilder3 = uiBuilder1;
                        LocaleString name = (LocaleString)subcategory2.Name;
                        color color = new color(1f, 1f, 0.8f, 1f);
                        ref color local3 = ref color;
                        ButtonEventHandler<string> callback = new ButtonEventHandler<string>(this.OnOpenCategoryPressed);
                        string str = path + "/" + subcategory2.Name;
                        uiBuilder3.Button<string>(name, in local3, callback, str, 0.35f);
                    }
                }
                HashSet<string> hashSet = Pool.BorrowHashSet<string>();
                foreach (System.Type element in subcategory1.Elements)
                {
                    if (!LogixHelper.IsHidden(element))
                    {
                        string overloadName = LogixHelper.GetOverloadName(element);
                        if (overloadName == null || hashSet.Add(overloadName))
                        {
                            System.Type type = element;
                            if (overloadName != null)
                                type = LogixHelper.GetMatchingOverload(overloadName, (NodeTypes)null, new Func<System.Type, int>(LogixNodeSelector2.GetTypeRank));
                            list.Add(new LogixNodeSelector2.ElementListing(LogixHelper.GetNodeName(type), type));
                        }
                    }
                }
                Pool.Return<string>(ref hashSet);
            }
            else
            {
                string directoryName = PathUtility.GetDirectoryName(path);
                UIBuilder uiBuilder3 = uiBuilder1;
                LocaleString text = (LocaleString)"(back)";
                color color = new color(1f, 0.8f, 0.8f, 1f);
                ref color local3 = ref color;
                ButtonEventHandler<string> callback1 = new ButtonEventHandler<string>(this.OnOpenCategoryPressed);
                string str1 = directoryName;
                uiBuilder3.Button<string>(text, in local3, callback1, str1, 0.35f);
                foreach (System.Type commonGenericType in WorkerInitializer.GetCommonGenericTypes(WorkerManager.GetType(PathUtility.GetFileName(path))))
                {
                    System.Type nodeVisualType = commonGenericType.GenericTypeArguments[0];
                    UIBuilder uiBuilder4 = uiBuilder1;
                    LocaleString niceName = (LocaleString)commonGenericType.GenericTypeArguments[0].GetNiceName("<", ">");
                    color = new color(0.8f, 1f, 0.9f, 1f);
                    if (nodeVisualType != (System.Type)null)
                        color = nodeVisualType.GetColor();
                    ref color local4 = ref color;
                    ButtonEventHandler<string> callback2 = new ButtonEventHandler<string>(this.OnSelectNodeTypePressed);
                    string str2 = BaseX.TypeHelper.TryGetAlias(commonGenericType) ?? commonGenericType.FullName;
                    Button button = uiBuilder4.Button<string>(niceName, in local4, callback2, str2, 0.35f);
                    if (nodeVisualType != (System.Type)null)
                    {
                        Image componentInChildren = button.Slot.GetComponentInChildren<Image>((Predicate<Image>)null, false);
                        componentInChildren.Sprite.Target = (IAssetProvider<Sprite>)LogixHelper.GetTypeSprite(this.World, nodeVisualType.GetDimensions(), typeof(Delegate).IsAssignableFrom(nodeVisualType));
                        componentInChildren.PreserveAspect.Value = false;
                    }
                    Slot s = button.Slot;
                    var buttonRelay = s.GetComponent<ButtonRelay<string>>();
                    ValueField<string> store = s.AttachComponent<ValueField<string>>();
                    Debug.Log(buttonRelay.Argument.Value);
                    //store.Value.Value = LogixHelper.GetNodeVisualType(WorkerManager.GetType(buttonRelay.Argument.Value)).ToString();

                    Type type = WorkerManager.GetType(buttonRelay.Argument.Value);
                    string fulltype = buttonRelay.Argument.Value;
                    int first_bracket = fulltype.IndexOf("[[");
                    store.Value.Value = fulltype.Substring(first_bracket + 2, fulltype.Length - 2 - first_bracket - 2);
                    DynamicValueVariable<string> dynamicvar = s.AttachComponent<DynamicValueVariable<string>>();
                    dynamicvar.VariableName.Value = "GenericType";
                    FormatString formatString = s.AttachComponent<FormatString>();
                    formatString.Parameters.Add();
                    formatString.Parameters.Add();
                    Cast_string_To_object cast1 = s.AttachComponent<Cast_string_To_object>();
                    cast1.In.Value = dynamicvar.Value.ReferenceID;
                    Cast_string_To_object cast2 = s.AttachComponent<Cast_string_To_object>();
                    cast2.In.Value = store.Value.ReferenceID;
                    formatString.Parameters[0].Value = cast1.ReferenceID;
                    formatString.Parameters[1].Value = cast2.ReferenceID;
                    ValueField<string> format = s.AttachComponent<ValueField<string>>();
                    format.Value.Value = "{0}[[{1}]]";
                    formatString.Format.Value = format.Value.ReferenceID;
                    DriverNode<string> driver = s.AttachComponent<DriverNode<string>>();
                    driver.Source.Value = formatString.Str.ReferenceID;
                    driver.DriveTarget.Target = buttonRelay.Argument;
                }
            }
            list.Sort();
            foreach (LogixNodeSelector2.ElementListing elementListing in list)
            {
                System.Type type = elementListing.type;
                System.Type nodeVisualType = LogixHelper.GetNodeVisualType(type);
                if (type.IsGenericTypeDefinition && WorkerInitializer.GetCommonGenericTypes(type).Any<System.Type>())
                {
                    UIBuilder uiBuilder3 = uiBuilder1;
                    LocaleString name = (LocaleString)elementListing.name;
                    color color = new color(0.8f, 1f, 0.8f, 1f);
                    ref color local3 = ref color;
                    ButtonEventHandler<string> callback = new ButtonEventHandler<string>(this.OpenGenericTypesPressed);
                    string str = Path.Combine(path, type.FullName);
                    uiBuilder3.Button<string>(name, in local3, callback, str, 0.35f);
                }
                else
                {
                    color tint = new color(0.8f, 0.9f, 1f, 1f);
                    if (nodeVisualType != (System.Type)null)
                        tint = nodeVisualType.GetColor();
                    Button button = uiBuilder1.Button<string>((LocaleString)elementListing.name, in tint, new ButtonEventHandler<string>(this.OnSelectNodeTypePressed), type.FullName, 0.35f);
                    if (nodeVisualType != (System.Type)null)
                    {
                        Image componentInChildren = button.Slot.GetComponentInChildren<Image>((Predicate<Image>)null, false);
                        componentInChildren.Sprite.Target = (IAssetProvider<Sprite>)LogixHelper.GetTypeSprite(this.World, nodeVisualType.GetDimensions(), typeof(Delegate).IsAssignableFrom(nodeVisualType));
                        componentInChildren.PreserveAspect.Value = false;
                    }
                }
            }
            Pool.Return<LogixNodeSelector2.ElementListing>(ref list);
        }

        private static int GetTypeRank(System.Type type)
        {
            System.Type type1 = type;
            if (typeof(IVector).IsAssignableFrom(type))
                type1 = type.GetVectorBaseType();
            if (type1 == typeof(dummy))
                return 0;
            if (type1 == typeof(float))
                return 1;
            return type1 == typeof(int) ? 2 : type.GetTypeRank() + 3;
        }

        private void OnOpenCategoryPressed(IButton button, ButtonEventData eventData, string path)
        {
            this.BuildUI(path, false);
        }

        private void OpenGenericTypesPressed(
          IButton button,
          ButtonEventData eventData,
          string pathType)
        {
            this.BuildUI(pathType, true);
        }

        private void OnSelectNodeTypePressed(
          IButton button,
          ButtonEventData eventData,
          string typename)
        {
            LogixTip tip = (this.LocalUserRoot.Slot.GetComponentInChildren<LogixTip>((Predicate<LogixTip>)(t => t.IsEquipped), false) ?? this.LocalUserRoot.Slot.GetComponentInChildren<LogixTip>((Predicate<LogixTip>)null, false));
            if (tip != (LogixTip) null)
            {
                MethodInfo privMethod = tip.GetType().GetMethod("SetActiveType", BindingFlags.NonPublic | BindingFlags.Instance);
                privMethod.Invoke(tip, new object[] { WorkerManager.GetType(typename) });
            }
        }


        private readonly struct ElementListing : IComparable<LogixNodeSelector2.ElementListing>, IEquatable<LogixNodeSelector2.ElementListing>
        {
            public readonly string name;
            public readonly System.Type type;

            public ElementListing(string name, System.Type type)
            {
                this.name = name;
                this.type = type;
            }

            public int CompareTo(LogixNodeSelector2.ElementListing other)
            {
                return this.name.CompareTo(other.name);
            }

            public bool Equals(LogixNodeSelector2.ElementListing other)
            {
                return this.name == other.name && this.type == other.type;
            }
        }
    }
}
