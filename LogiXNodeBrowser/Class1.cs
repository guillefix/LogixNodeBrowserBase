using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrooxEngine;
using FrooxEngine.UIX;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using BaseX;
using FrooxEngine.LogiX;
using FrooxEngine.LogiX.Data;
using FrooxEngine.LogiX.String;
using FrooxEngine.LogiX.Cast;
using FrooxEngine.LogiX.WorldModel;
using FrooxEngine.LogiX.ProgramFlow;
using FrooxEngine.LogiX.Operators;

namespace FrooxEngine.LogiX
{
    [Category("LogiX/Builders")]
    [NodeName("Build Node Browser")]
    public class BuildNodeBrowser : LogixNode
    {
        public HashSet<string> hashSet = Pool.BorrowHashSet<string>();
        public LogixNodeSelector2 logixNodeSelector;
        public Slot logixHierarchy, componentHierarchy;
        public Slot mainSlot;
        public string components_string;
        public string logix_string;
        public string components_string2;
        public string logix_string2;
        public Slot nodesContainerSlot;
        public StringRegister searchResponse;
        public string sep;
        protected override void OnAttach()
        {
            base.OnAttach();
            sep = "\t";
            //CONTAINING SLOT AND BASE CANVAS
            mainSlot = this.LocalUserSpace.AddSlot("LogiX Node Browser");
            NeosPanel neosPanel = mainSlot.AttachComponent<NeosPanel>(true, (Action<NeosPanel>)null);
            neosPanel.Title = "Logix/Componer browser";
            neosPanel.Padding.Value = 0.005f;
            neosPanel.ZPadding.Value = 0.005f;
            neosPanel.Thickness.Value = 0.01f;
            neosPanel.AddCloseButton();
            neosPanel.AddParentButton();
            UIBuilder uiBuilder1 = new UIBuilder(neosPanel.ContentSlot, 1000f, 1000f, 0.0005f);
            //Canvas component = neosPanel.ContentSlot.GetComponent<Canvas>((Predicate<Canvas>)null, false);
            color color1 = new color(1f, 1f, 1f, 0.2f);
            uiBuilder1.Panel(in color1, false);

            //Search Footer
            RectTransform searchFooter, mainContent;
            uiBuilder1.HorizontalFooter(64f, out searchFooter, out mainContent);
            UIBuilder searchUI = new UIBuilder(searchFooter);
            TextField textfield = searchUI.TextField();
            Action sendRequest = this.BuildSearchLogix(textfield.Text.Content.ReferenceID);
            var editorEvents = textfield.Slot.AddSlot("logix").AttachComponent<Interaction.TextEditorEvents>();
            editorEvents.Source.Value = textfield.Editor;
            editorEvents.EditingFinished.Target = sendRequest;


            //MAIN CONTENT
            //Horizontal split
            List<RectTransform> rectTransformList = (new UIBuilder(mainContent)).SplitHorizontally(0.4f, 0.6f);

            //RIGHT SIDE
            Slot rightSlot = rectTransformList[1].Slot;
            logixNodeSelector = rightSlot.AttachComponent<LogixNodeSelector2>();
            Slot contentSlot = rightSlot.FindChild(s => s.Name == "Content");
            Slot containerSlot = contentSlot[0];
            nodesContainerSlot = containerSlot;
            contentSlot.ActiveSelf = false;
            rightSlot.FindChild(s => s.Name == "Panel").Destroy();
            rightSlot.FindChild(s => s.Name == "Handle").Destroy();
            rightSlot.FindChild(s => s.Name == "Header").Destroy();
            rightSlot.FindChild(s => s.Name == "HandleAnchor").Destroy();
            containerSlot.Parent = rightSlot;


            //LEFT SIDE
            RectTransform hierarchyHeader, nodeHierarchy;
            Slot leftSlot = rectTransformList[0].Slot;
            UIBuilder uiBuilder2 = new UIBuilder(leftSlot);

            //header
            uiBuilder2.HorizontalHeader(64f, out hierarchyHeader, out nodeHierarchy);
            UIBuilder uiBuilder3 = new UIBuilder(hierarchyHeader);
            uiBuilder3.HorizontalLayout(4f, 0.0f, 4f, 0.0f, 4f, new Alignment?());
            uiBuilder3.Style.FlexibleWidth = 100f;
            //hierarchyHeader = uiBuilder3.CurrentRect;
            uiBuilder3.HorizontalLayout(4f, 0.0f, 0.0f, 4.0f, 0.0f);

            //hierarchy container
            UIBuilder uiBuilderContent = new UIBuilder(nodeHierarchy);
            color color2 = new color(1f, 1f, 1f, 0.2f);
            uiBuilderContent.Panel(in color2, false);
            //nodeHierarchy = uiBuilderContent.CurrentRect;
            uiBuilderContent.ScrollArea(new Alignment?());
            uiBuilderContent.VerticalLayout(4.0f, 0.0f, Alignment.TopLeft);
            uiBuilderContent.FitContent(SizeFit.Disabled, SizeFit.MinSize);
            componentHierarchy = uiBuilderContent.CurrentRect.Slot;
            logixHierarchy = componentHierarchy.Duplicate();

            //Logix/Comp buttons
            uiBuilder3.Style.FlexibleWidth = -1f;
            uiBuilder3.Style.MinWidth = 64f;
            Button logixButton = uiBuilder3.Button("LogiX");
            var component1 = logixButton.Slot.AttachComponent<ButtonValueSet<bool>>();
            component1.TargetValue.Target = logixHierarchy.ActiveSelf_Field;
            component1.SetValue.Value = true;
            var component2 = logixButton.Slot.AttachComponent<ButtonValueSet<bool>>();
            component2.TargetValue.Target = componentHierarchy.ActiveSelf_Field;
            component2.SetValue.Value = false;
            Button compButton = uiBuilder3.Button("Comps");
            var component3 = compButton.Slot.AttachComponent<ButtonValueSet<bool>>();
            component3.TargetValue.Target = logixHierarchy.ActiveSelf_Field;
            component3.SetValue.Value = false;
            var component4 = compButton.Slot.AttachComponent<ButtonValueSet<bool>>();
            component4.TargetValue.Target = componentHierarchy.ActiveSelf_Field;
            component4.SetValue.Value = true;

            components_string2 = "NiceName"+sep+"Name"+sep+"FullName"+sep+"Folder"+sep+"Description"+sep+"Type\n";
            logix_string2 = components_string2;
            buildLayer("",componentHierarchy);
            buildLayer("/LogiX",logixHierarchy);

            componentHierarchy.ActiveSelf = false;
            logixHierarchy.ActiveSelf = true;

            components_string += "\n ";
            logix_string += "\n ";
            //components_string2 += "\n ";
            //logix_string2 += "\n ";
            System.IO.File.WriteAllText(@"C:\Program Files (x86)\Steam\steamapps\common\NeosVR\components_string.txt", components_string);
            System.IO.File.WriteAllText(@"C:\Program Files (x86)\Steam\steamapps\common\NeosVR\logix_string.txt", logix_string);
            System.IO.File.WriteAllText(@"C:\Program Files (x86)\Steam\steamapps\common\NeosVR\components_string2.txt", components_string2);
            System.IO.File.WriteAllText(@"C:\Program Files (x86)\Steam\steamapps\common\NeosVR\logix_string2.txt", logix_string2);

            //this.MakeVariableSelector();

        }

        public Slot buildLayerUI(string name, string type, Slot slot, int depth)
        {
            UIBuilder uiBuilder3 = new UIBuilder(slot);
            uiBuilder3.Style.ForceExpandHeight = false;
            uiBuilder3.Style.ChildAlignment = Alignment.TopLeft;
            uiBuilder3.HorizontalLayout(4f, 0.0f, new Alignment?());
            uiBuilder3.Style.MinHeight = 32f;
            uiBuilder3.Style.MinWidth = 32f;
            Button button1 = uiBuilder3.Button();
            Expander expander = button1.Slot.AttachComponent<Expander>(true);
            TextExpandIndicator textExpandIndicator = button1.Slot.AttachComponent<TextExpandIndicator>(true);
            textExpandIndicator.Text.Target = (IField<string>)button1.Slot.GetComponentInChildren<Text>((Predicate<Text>)null, false).Content;
            uiBuilder3.Style.FlexibleWidth = 100f;
            Text text = uiBuilder3.Text(name, true, Alignment.MiddleLeft, true, (string)null);
            Button button2 = text.Slot.AttachComponent<Button>(true, (Action<Button>)null);
            MethodInfo privMethod = logixNodeSelector.GetType().GetMethod("OnOpenCategoryPressed", BindingFlags.NonPublic | BindingFlags.Instance);
            ButtonEventHandler<string> action = (ButtonEventHandler<string>)Delegate.CreateDelegate(typeof(ButtonEventHandler<string>), logixNodeSelector, privMethod);
            button2.SetupAction<string>(action, type, 0.3f);
            InteractionElement.ColorDriver colorDriver = button2.ColorDrivers.Add();
            colorDriver.ColorDrive.Target = (IField<color>)text.Color;
            colorDriver.NormalColor.Value = color.Black;
            colorDriver.HighlightColor.Value = color.Blue;
            colorDriver.PressColor.Value = color.Cyan;
            //button2.SetupAction(new ButtonEventHandler(this.OnSlotNamePressed), 0.0f);
            //this._slotNameText.Target = text;
            uiBuilder3.Style.FlexibleWidth = -1f;
            uiBuilder3.NestOut();
            uiBuilder3.Style.MinHeight = -1f;
            HorizontalLayout horizontalLayout = uiBuilder3.HorizontalLayout(4f, 0.0f, new Alignment?());
            uiBuilder3.Style.MinWidth = 32f;
            uiBuilder3.Empty("Spacer");
            uiBuilder3.Style.FlexibleWidth = 100f;
            //uiBuilder3.VerticalLayout(4f, 0.0f, new Alignment?());
            Slot childrenSlot = uiBuilder3.VerticalLayout(4f, 0.0f, new Alignment?()).Slot;
            expander.SectionRoot.Target = horizontalLayout.Slot;
            textExpandIndicator.SectionRoot.Target = horizontalLayout.Slot;
            textExpandIndicator.ChildrenRoot.Target = childrenSlot;
            //if (depth<2) buildHierarchyLevel(childrenSlot, depth+1);
            expander.IsExpanded = false;
            //slot.ActiveSelf = depth == 0;
            return childrenSlot;
        }

        public void buildLayer(string path, Slot slot)
        {
            int depth = path.Count(f => f == '/');
            string top_level;
            if (path == "")
                top_level = "";
            else
                top_level = path.Split('/')[1];
            //string indentation_string = new string(':', depth) + (depth>0 ? " " : "");
            string indentation_string = new string(':', depth) + " ";
            CategoryNode<System.Type> categoryNode;
            if (string.IsNullOrEmpty(path) || path == "/")
            {
                categoryNode = WorkerInitializer.ComponentLibrary;
            }
            else
            {
                categoryNode = WorkerInitializer.ComponentLibrary.GetSubcategory(path);
            }
            foreach (CategoryNode<System.Type> subcategory in categoryNode.Subcategories.ToList())
            {
                string text = subcategory.Name;
                string path_str = path + "/" + subcategory.Name;
                if (top_level=="LogiX") logix_string += indentation_string + text + "\n";
                else components_string += indentation_string + text + "\n";
                if (top_level=="LogiX") logix_string2 += text + sep + text + sep + path + sep + sep+"folder\n";
                else components_string2 += text + sep + text + sep + path_str + sep + path + sep + sep + "folder\n";
                Debug.Log(path_str);
                if (subcategory.Name != "LogiX")
                {
                    Slot childrenSlot = buildLayerUI(text,path_str,slot,depth);
                    buildLayer(path_str, childrenSlot);
                }
                //else
                //{
                //    addToOutput(str);
                //}

            }
            foreach (System.Type element in categoryNode.Elements.ToList())
            {
                if (top_level == "LogiX")
                //if (false)
                {
                    //Debug.Log("HIIIII");
                    string overloadName = LogixHelper.GetOverloadName(element);
                    if (overloadName == null || hashSet.Add(overloadName))
                    {
                        string niceName = element.GetNiceName("<", ">");
                        string fullName = element.FullName;
                        Debug.Log(niceName);
                        if (overloadName != null) logix_string += indentation_string + overloadName + ":" + fullName + "\n";
                        //else components_string += indentation_string + niceName + "\n";
                        else logix_string += indentation_string + niceName + ":" + fullName + "\n";

                        if (overloadName != null) logix_string2 += LogixHelper.GetNodeName(element) + sep + overloadName + sep + fullName + sep + path + sep + sep + "normal" + "\n";
                        //else components_string += indentation_string + niceName + "\n";
                        else
                        {
                            if (element.IsGenericTypeDefinition)
                            {
                                logix_string2 += LogixHelper.GetNodeName(element) + sep + niceName + sep + fullName + sep + path + sep+sep+"generic\n";
                            } else logix_string2 += LogixHelper.GetNodeName(element) + sep + niceName + sep + fullName + sep + path + sep+sep+"normal\n";
                        }
                    }
                }
                else
                {
                    string niceName = element.GetNiceName("<", ">");
                    string fullName = element.FullName;
                    Debug.Log(niceName);
                    components_string += indentation_string + niceName + ":" + fullName + "\n";
                    if (element.IsGenericTypeDefinition)
                    {
                        components_string2 += niceName + sep + niceName + sep + fullName + sep + path + sep+sep+"generic"+ "\n";
                    } else
                    {
                        components_string2 += niceName + sep + niceName + sep + fullName + sep + path + sep+sep+"normal"+"\n";
                    }
                }
            }
        }

        public Action BuildSearchLogix(RefID query)
        {
            Slot logix = mainSlot.AddSlot("Search logix");
            Network.GET_String getString = logix.AttachComponent<Network.GET_String>();
            StringRegister url = logix.AttachComponent<StringRegister>();
            url.Value.Value = "https://better-lightning-crate.glitch.me/";
            FormatString formatString = logix.AttachComponent<FormatString>();
            ValueField<string> type = logix.AttachComponent<ValueField<string>>();
            ValueField<string> format = logix.AttachComponent<ValueField<string>>();
            format.Value.Value = "{0}/{1}/{2}";
            formatString.Format.Value = format.Value.ReferenceID;
            formatString.Parameters.Add();
            formatString.Parameters.Add();
            formatString.Parameters.Add();
            formatString.Parameters[0].Value = url.Value.ReferenceID;
            formatString.Parameters[1].Value = type.Value.ReferenceID;
            formatString.Parameters[2].Value = query;
            getString.URL.Value = formatString.Str.ReferenceID;
            Actions.WriteValueNode<string> writeResponse = logix.AttachComponent<Actions.WriteValueNode<string>>();
            StringRegister response = logix.AttachComponent<StringRegister>();
            searchResponse = response;
            writeResponse.Value.Value = getString.Content.ReferenceID;
            writeResponse.Target.Value = response.Value.ReferenceID;
            getString.OnResponse.Target = writeResponse.Write;
            //writeResponse.Target.OwnerNode.RemoveAllLogixBoxes();
            //LogixHelper.MoveUnder(writeResponse.Target.OwnerNode, logix, true);
            //getString.Request();
            return getString.Request;
        }

        [ImpulseTarget]
        protected void PackLogix()
        {
            Slot logixSlot = mainSlot.FindChild(s => s.Name == "Search logix");
            LogixNode node = logixSlot.GetComponentInChildrenOrParents<LogixNode>();
            LogixHelper.MoveUnder(node, logixSlot, true);
            node.RemoveAllLogixBoxes();
        }

        [ImpulseTarget]
        public void MakeVariableSelector()
        {
            MethodInfo privMethod = logixNodeSelector.GetType().GetMethod("BuildUI", BindingFlags.NonPublic | BindingFlags.Instance);
            //privMethod.Invoke(logixNodeSelector,new object[] { "LogiX/Variables" });
            //FieldInfo fi = typeof(LogixNodeSelector).GetField("_container", BindingFlags.NonPublic | BindingFlags.Instance);
            //Slot container = (Slot)fi.GetValue(logixNodeSelector);
            Slot variableNodesContainer = nodesContainerSlot[0][0][0];
            Slot typeSelectTemplate = mainSlot.AddSlot("Type select template");
            Slot variableNodesContainer2 = variableNodesContainer.Duplicate();
            variableNodesContainer2.AttachComponent<DynamicVariableSpace>();
            DynamicValueVariable<string> genericType = variableNodesContainer2.AttachComponent<DynamicValueVariable<string>>();
            genericType.VariableName.Value = "GenericType";
            variableNodesContainer2.SetParent(typeSelectTemplate);
            variableNodesContainer2[0].Destroy();
            for (int i = 0; i < variableNodesContainer2.ChildrenCount; i++)
            {
                Debug.Log(variableNodesContainer2[i]);
                Slot s = variableNodesContainer2[i];
                var buttonRelay = s.GetComponent<ButtonRelay<string>>();
                ValueField<string> store = s.AttachComponent<ValueField<string>>();
                Debug.Log(buttonRelay.Argument.Value);
                //store.Value.Value = LogixHelper.GetNodeVisualType(WorkerManager.GetType(buttonRelay.Argument.Value)).ToString();

                Type type = WorkerManager.GetType(buttonRelay.Argument.Value);
                Image componentInChildren = s.GetComponentInChildren<Image>((Predicate<Image>)null, false);
                System.Type nodeVisualType = LogixHelper.GetNodeVisualType(type);
                componentInChildren.Sprite.Target = (IAssetProvider<Sprite>)LogixHelper.GetTypeSprite(this.World, nodeVisualType.GetDimensions(), typeof(Delegate).IsAssignableFrom(nodeVisualType));
                componentInChildren.PreserveAspect.Value = false;

                store.Value.Value = BaseX.TypeHelper.TryGetAlias(type) ?? type.FullName;
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
                format.Value.Value = "{0}[{1}]";
                formatString.Format.Value = format.Value.ReferenceID;
                DriverNode<string> driver = s.AttachComponent<DriverNode<string>>();
                driver.Source.Value = formatString.Str.ReferenceID;
                driver.DriveTarget.Target = buttonRelay.Argument;
            }
            //this.RunSynchronously(() =>
            //{
            //    variableNodesContainer2.ForeachChild((Action<FrooxEngine.Slot>) (s =>
            //    {
            //        Debug.Log(s);
            //        //var buttonRelay = s.GetComponent<ButtonRelay<string>>();
            //        //Debug.Log(buttonRelay.ToString());
            //        //ValueField<string> store = s.AttachComponent<ValueField<string>>();
            //        //store.Value.Value = LogixHelper.GetNodeVisualType(WorkerManager.GetType(buttonRelay.Argument.Value)).ToString();
            //        //store.Value.Value = Type.GetType(buttonRelay.Argument.Value).GetContentType().ToString();
            //        //Debug.Log(buttonRelay.Argument.Value);
            //        //store.Value.Value = buttonRelay.Argument;
            //        //store.Value.Value = typeof(bool2).ToString();
            //        //DynamicValueVariable<string> dynamicvar = s.AttachComponent<DynamicValueVariable<string>>();
            //        //dynamicvar.VariableName.Value = "GenericType";
            //        //FormatString formatString = s.AttachComponent<FormatString>();
            //        //formatString.Parameters.Add();
            //        //formatString.Parameters.Add();
            //        //Cast_string_To_object cast1 = s.AttachComponent<Cast_string_To_object>();
            //        //cast1.In.Value = dynamicvar.Value.ReferenceID;
            //        //Cast_string_To_object cast2 = s.AttachComponent<Cast_string_To_object>();
            //        //cast2.In.Value = store.Value.ReferenceID;
            //        //formatString.Parameters[0].Value = cast1.ReferenceID;
            //        //formatString.Parameters[1].Value = cast2.ReferenceID;
            //        //ValueField<string> format = s.AttachComponent<ValueField<string>>();
            //        //format.Value.Value = "{0}[{1}]";
            //        //formatString.Format.Value = format.Value.ReferenceID;
            //        //DriverNode<string> driver = s.AttachComponent<DriverNode<string>>();
            //        //driver.Source.Value = formatString.Str.ReferenceID;
            //        //driver.DriveTarget.Target = buttonRelay.Argument;
            //    }));
            //});
            privMethod.Invoke(logixNodeSelector,new object[] { "LogiX", false });
        }

        public void MakeUpdateResultsLogix()
        {
            Slot nodesContainer = nodesContainerSlot[0][0][0];
            Slot nodeTemplate = mainSlot.AddSlot("NodeTemplate");
            Slot template = nodesContainer[nodesContainer.ChildrenCount - 1].Duplicate();
            template.SetParent(nodeTemplate);
            ButtonRelay<string> buttonRelay = template.GetComponent<ButtonRelay<string>>();
            Slot templateLogix = template.AddSlot("logix");
            Actions.WriteValueNode<string> writeThing = templateLogix.AttachComponent<Actions.WriteValueNode<string>>();
            DynamicImpulseReceiverWithValue<string> receiver = templateLogix.AttachComponent<DynamicImpulseReceiverWithValue<string>>();
            ValueField<string> tag = templateLogix.AttachComponent<ValueField<string>>();
            tag.Value.Value = "setUpNode";
            receiver.Tag.Value = tag.Value.ReferenceID;
            writeThing.Value.Value = receiver.Value.ReferenceID;
            ValueField<string> response = templateLogix.AttachComponent<ValueField<string>>();
            writeThing.Target.Value = response.Value.ReferenceID;
            receiver.Impulse.Target = writeThing.Write;
            ForNode iterator = templateLogix.AttachComponent<ForNode>();

            Slot logix = templateLogix;
            Substring getResultSubstring = logix.AttachComponent<Substring>();
            IndexOfString findSeparator = logix.AttachComponent<IndexOfString>();
            getResultSubstring.Str.Value = searchResponse.Value.ReferenceID;
            findSeparator.Str.Value = searchResponse.Value.ReferenceID;
            ValueField<int> currentIndex = logix.AttachComponent<ValueField<int>>();
            getResultSubstring.StartIndex.Value = currentIndex.Value.ReferenceID;
            findSeparator.StartIndex.Value = currentIndex.Value.ReferenceID;
            ValueField<string> separator = logix.AttachComponent<ValueField<string>>();
            separator.Value.Value = ",";
            findSeparator.Part.Value = separator.Value.ReferenceID;
            Sub_Int lengthOfPart = logix.AttachComponent<Sub_Int>();
            lengthOfPart.A.Value = findSeparator.ReferenceID;
            lengthOfPart.B.Value = currentIndex.Value.ReferenceID;
            getResultSubstring.Length.Value = lengthOfPart.ReferenceID;


            //Slot logix = mainSlot.AddSlot("Update search results logix");
            //ReferenceField<Slot> containerSlotStore = logix.AttachComponent<ReferenceField<Slot>>();
            //containerSlotStore.Reference.Value = nodesContainer.ReferenceID;
            //DestroySlotChildren destroyContainerContents = logix.AttachComponent<DestroySlotChildren>();
            //destroyContainerContents.Instance.Value = nodesContainer.ReferenceID;

            //WhileNode whileNode = logix.AttachComponent<WhileNode>();
            //Substring getResultSubstring = logix.AttachComponent<Substring>();
            //IndexOfString findSeparator = logix.AttachComponent<IndexOfString>();
            //getResultSubstring.Str.Value = searchResponse.Value.ReferenceID;
            //findSeparator.Str.Value = searchResponse.Value.ReferenceID;
            //ValueField<int> currentIndex = logix.AttachComponent<ValueField<int>>();
            //getResultSubstring.StartIndex.Value = currentIndex.Value.ReferenceID;
            //findSeparator.StartIndex.Value = currentIndex.Value.ReferenceID;
            //ValueField<string> separator = logix.AttachComponent<ValueField<string>>();
            //separator.Value.Value = "|";
            //findSeparator.Part.Value = separator.Value.ReferenceID;
            //Sub_Int lengthOfPart = logix.AttachComponent<Sub_Int>();
            //lengthOfPart.A.Value = findSeparator.ReferenceID;
            //lengthOfPart.B.Value = currentIndex.Value.ReferenceID;
            //getResultSubstring.Length.Value = lengthOfPart.ReferenceID;
            //DynamicImpulseTriggerWithValue<string> receive_part = logix.AttachComponent<DynamicImpulseTriggerWithValue<string>>();
            
        }

    }
}

