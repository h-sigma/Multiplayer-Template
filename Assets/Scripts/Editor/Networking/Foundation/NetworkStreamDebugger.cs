using System;
using System.Collections.Generic;
using System.Linq;
using Editor.Utility;
using Networking.Foundation;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utility;

namespace Editor.Networking.Foundation
{
    public class NetworkStreamDebugger : EditorWindow
    {
        #region Class Definitions

        public abstract class StreamSnapshotBase : VisualElement
        {
            public abstract void RefreshUI();
            public abstract void ExpireAllItems();
            public abstract void AddMockDataToStream();
        }

        public class StreamSnapshot<TStruct> : StreamSnapshotBase where TStruct : struct, IPacketSerializable
        {
            public StreamSnapshot()
            {
                AddToClassList(s_StreamUssClass);
                this.name = "StreamSnapshot<" + typeof(TStruct).Name + ">";
            }

            #region StreamSnapshotBase

            public override void ExpireAllItems()
            {
                var enumerator = NetworkStream<TStruct>.Read();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        var item = enumerator.Current;
                        item.Status = StreamItemStatus.Expired;
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Debug.Log(ex.Message);
                }

                RefreshUI();
            }

            public override void AddMockDataToStream()
            {
                var item = new TStruct();
                NetworkStream<TStruct>.Enqueue(ref item);
                NetworkStreamController.UpdateStreams();
                RefreshUI();
            }

            public override void RefreshUI()
            {
                Clear();

                AddHeader();

                var enumerator = NetworkStream<TStruct>.Read();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        var item = enumerator.Current;
                        AddAsChild(item);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    Debug.Log(ex.Message);
                }
            }

            #endregion

            #region Privates

            private void AddHeader()
            {
                var header = new VisualElement();
                header.AddToClassList(s_StreamRowUssClass);

                header.Add(new Label("Time").AddClasses(s_TimeUssClass));
                header.Add(new Label("Data").AddClasses(s_DataUssClass));
                header.Add(new Label("Status").AddClasses(s_StatusUssClass));

                this.Add(header);
            }

            private void AddAsChild(NetworkStream<TStruct>.DataWrapper data)
            {
                var row = new VisualElement();
                row.AddToClassList(s_StreamRowUssClass);
                this.Add(row);

                //time
                var time = new Label(new DateTime(data.TimeOfArrival).ToString("HH:mm:ss")).AddClasses(s_TimeUssClass);
                row.Add(time);
                
                //data
                {
                    //text data
                    var dataAsJson = EditorJsonUtility.ToJson(data.Data, true);

                    var textData = new TextField();
                    textData.multiline = true;
                    textData.labelElement.visible = false;
                    textData.value                = dataAsJson;
                    textData.style.flexGrow = new StyleFloat(1);
                    
                    //update button
                    var updateButton = new Button(() =>
                    {
                        data.Data = JsonUtility.FromJson<TStruct>(textData.value);
                        textData.value = JsonUtility.ToJson(data.Data, true);
                    }){text = "Update"};
                    updateButton.style.alignSelf = Align.Center;

                    var localParent = new VisualElement().AddClasses(s_DataUssClass);
                    localParent.Add(textData);
                    localParent.Add(updateButton);
                    row.Add(localParent);
                }

                //status
                {
                    var status = new EnumField(data.Status).AddClasses(s_StatusUssClass);
                    status.labelElement.visible = false;
                    status.style.alignSelf = Align.Center;

                    status.RegisterValueChangedCallback(evt =>
                    {
                        if(data.Status == (evt.previousValue is StreamItemStatus ? (StreamItemStatus) evt.previousValue : StreamItemStatus.Stable))
                            data.Status = evt.newValue is StreamItemStatus ? (StreamItemStatus) evt.newValue : StreamItemStatus.Stable;
                    });
                    
                    row.Add(status);
                }
            }

            #endregion
        }

        #endregion

        [MenuItem("Debug/Network Stream")]
        private static void ShowWindow()
        {
            var window = GetWindow<NetworkStreamDebugger>();
            window.titleContent = new GUIContent("Network Stream Debugger");
            window.Show();
        }

        public static readonly string s_TimeUssClass   = "time";
        public static readonly string s_DataUssClass   = "data";
        public static readonly string s_StatusUssClass = "status";

        public static readonly string s_StreamUssClass           = "stream";
        public static readonly string s_StreamsContainerUssClass = "stream__container";
        public static readonly string s_StreamRowUssClass        = "stream__row";
        public static readonly string s_ButtonUssClass           = "button";
        public static readonly string s_ButtonListUssClass       = "buttonlist";
        public static readonly string s_TabListUssClass          = "tablist";
        public static readonly string s_TabUssClass              = "tab";


        private VisualElement            _root;
        private List<StreamSnapshotBase> _snapshots;

        private VisualElement _buttonRoot;
        private Button        _updateStreamsButton;
        private Button        _playButton;
        private Button        _addFakeDataButton;
        private Button        _expireAllButton;

        private VisualElement _tabsRoot;

        private void OnEnable()
        {
            //add local root
            _root = new VisualElement() {name = "window_root"};
            rootVisualElement.Add(_root);
            //add stylesheet
            var stylesheet = Resources.Load<StyleSheet>("USS/" + nameof(NetworkStreamDebugger) + "_USS");
            _root.styleSheets.Add(stylesheet);

            AddButtonsAndScheduler();

            AddStreams();

            void AddButtonsAndScheduler()
            {
                var refreshScheduleItem = rootVisualElement.schedule.Execute(UpdateStreamSnapshot);
                refreshScheduleItem.Every(1000);

                _buttonRoot = new VisualElement() {name = "Button Root"};

                _updateStreamsButton = new Button(() => { NetworkStreamController.UpdateStreams(); }) {text = "Update Streams"};
                _updateStreamsButton.AddToClassList(s_ButtonUssClass);
                
                _playButton = new Button(() =>
                {
                    if (refreshScheduleItem.isActive)
                    {
                        refreshScheduleItem.Pause();
                        _playButton.text = "Play!";
                        
                        if(EditorApplication.isPlaying) EditorApplication.isPaused = true;
                    }
                    else
                    {
                        refreshScheduleItem.Resume();
                        _playButton.text = "Pause!";

                        if(EditorApplication.isPlaying) EditorApplication.isPaused = false;
                    }
                }) {text = refreshScheduleItem.isActive ? "Pause!" : "Play!"};
                _playButton.AddToClassList(s_ButtonUssClass);

                _addFakeDataButton = new Button(() =>
                {
                    foreach (var snap in _snapshots)
                    {
                        snap.AddMockDataToStream();
                    }
                }) {text = "Add Fake"};
                _addFakeDataButton.AddToClassList(s_ButtonUssClass);

                _expireAllButton = new Button(() =>
                {
                    foreach (var snap in _snapshots)
                    {
                        snap.ExpireAllItems();
                    }
                }) {text = "Expire All"};
                _expireAllButton.AddToClassList(s_ButtonUssClass);

                _root.Add(_buttonRoot);
                
                var leftSide = new VisualElement().AddClasses(s_ButtonListUssClass);
                var rightSide = new VisualElement().AddClasses(s_ButtonListUssClass);
                
                _buttonRoot.Add(leftSide);
                _buttonRoot.Add(rightSide);

                _buttonRoot.style.flexDirection = FlexDirection.Row;
                _buttonRoot.style.justifyContent = Justify.SpaceBetween;

                rightSide.style.flexDirection = FlexDirection.RowReverse;
                
                leftSide.Add(_playButton);
                leftSide.Add(_updateStreamsButton);
                
                rightSide.Add(_addFakeDataButton);
                rightSide.Add(_expireAllButton);
            }

            void AddStreams()
            {
                var snapshotsContainer = new VisualElement() {name = "Snapshots Container"};
                snapshotsContainer.AddToClassList(s_StreamsContainerUssClass);

                //manually add all streams
                _snapshots = new List<StreamSnapshotBase>()
                {
                    new StreamSnapshot<MatchData>()
                };

                //
                foreach (var snap in _snapshots)
                {
                    _root.Add(snap);
                }
            }
        }

        private void UpdateStreamSnapshot()
        {
            foreach (var snap in _snapshots)
            {
                snap.RefreshUI();
            }
        }
    }
}