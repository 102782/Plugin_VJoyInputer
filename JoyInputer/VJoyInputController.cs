using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections;
using System.IO;
using System.Threading;
using System.Windows.Forms;


namespace JoyInputer
{
    class VJoyInputController
    {
        private SimpleLogger logger;
        private VJoyController vjoy;
        private Stopwatch timer;
        private TimeSpan span;
        private List<Inputer> buttons;
        private List<Inputer> axis;
        private List<Inputer> pov;
        private SimpleQueue<string> sourceQueue;
        private Thread thread;
        private long axisMaxValue;

        public bool isInitializedVJoy { get; private set; }

        public VJoyInputController(SimpleLogger logger)
        {
            this.timer = new Stopwatch();
            this.logger = logger;
            this.sourceQueue = new SimpleQueue<string>();
        }

        private bool InitializeVJoy(string installedDir)
        {
            this.isInitializedVJoy = false;
            this.logger.Add(SimpleLogger.TAG.INFO, "Begin: Initialize VJoyController");
            if (!File.Exists(installedDir + "/vJoyInterface.dll"))
            {
                this.logger.Add(SimpleLogger.TAG.ERROR, installedDir + "vJoyInterface.dll not found.Exit!");
                return this.isInitializedVJoy;
            }

            if (!File.Exists(installedDir + "/vJoyInterfaceWrap.dll"))
            {
                this.logger.Add(SimpleLogger.TAG.ERROR, installedDir + "vJoyInterfaceWrap.dll not found.Exit!");
                return this.isInitializedVJoy;
            }

            this.vjoy = new VJoyController(1);
            if (!this.vjoy.CheckDeviceID())
            {
                this.logger.Add(SimpleLogger.TAG.ERROR, string.Format("Illegal device ID {0}.Exit!", this.vjoy.id));
                return this.isInitializedVJoy;
            }
            this.logger.Add(SimpleLogger.TAG.INFO, this.vjoy.GetDriverAttributes());
            this.logger.Add(SimpleLogger.TAG.INFO, this.vjoy.GetDeviceStatusMessage());
            if (this.vjoy.Acquire())
            {
                this.logger.Add(SimpleLogger.TAG.INFO, string.Format("Acquired: vJoy device number {0}.", this.vjoy.id));
            }
            else
            {
                this.logger.Add(SimpleLogger.TAG.ERROR, string.Format("Failed to acquire vJoy device number {0}.Exit!", this.vjoy.id));
                return this.isInitializedVJoy;
            }

            var axis = this.vjoy.GetAxisExist().ToArray();
            this.logger.Add(SimpleLogger.TAG.INFO, string.Format("Axis X {0}", axis[0] ? "Yes" : "No"));
            this.logger.Add(SimpleLogger.TAG.INFO, string.Format("Axis Y {0}", axis[1] ? "Yes" : "No"));
            this.logger.Add(SimpleLogger.TAG.INFO, string.Format("Axis Z {0}", axis[2] ? "Yes" : "No"));
            this.logger.Add(SimpleLogger.TAG.INFO, string.Format("Axis Rz {0}", axis[3] ? "Yes" : "No"));
            this.axisMaxValue = 0;
            this.vjoy.GetAxisMaxValue(ref axisMaxValue);
            this.logger.Add(SimpleLogger.TAG.INFO, string.Format("Max of Axis value {0}.", this.axisMaxValue));
            this.logger.Add(SimpleLogger.TAG.INFO, string.Format("Numner of buttons {0}.", this.vjoy.GetButtonNumber()));
            this.logger.Add(SimpleLogger.TAG.INFO, string.Format("Numner of Descrete POVs {0}.", this.vjoy.GetDiscPovNumber()));

            this.isInitializedVJoy = true;

            this.logger.Add(SimpleLogger.TAG.INFO, "End: Initialize VJoyController");

            return this.isInitializedVJoy;
        }

        public void Initialize(string installedDir, int span,
            IEnumerable<string> buttonPatterns,
            IEnumerable<string> axisPatterns,
            IEnumerable<string> povPatterns)
        {
            if (!InitializeVJoy(installedDir))
            {
                string text = "vJoyの初期化に失敗しました。" + Environment.NewLine
                     + "-----Log-----" + Environment.NewLine + this.logger.ToString();
                var result = MessageBox.Show(text, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            this.span = TimeSpan.FromMilliseconds(span);
            int buttonNumber = this.vjoy.GetButtonNumber();
            var axisExist = this.vjoy.GetAxisExist().ToArray();
            var axisSetting = new bool[] { axisExist[0], axisExist[1], axisExist[0], axisExist[1], axisExist[2], axisExist[3], axisExist[2], axisExist[3]};
            bool discPovExist = this.vjoy.GetDiscPovNumber() > 0;
            this.buttons = buttonPatterns.Select((p, i) => { return new Inputer(i, i < buttonNumber, TimeSpan.FromMilliseconds(0), p); }).ToList<Inputer>();
            this.axis = axisPatterns.Select((p, i) => { return new Inputer(i, axisSetting[i], TimeSpan.FromMilliseconds(0), p); }).ToList<Inputer>();
            this.pov = povPatterns.Select((p, i) => { return new Inputer(i, discPovExist, TimeSpan.FromMilliseconds(0), p); }).ToList<Inputer>();

            this.thread = new Thread(new ThreadStart(this.Update));
            this.thread.Start();
            this.logger.Add(SimpleLogger.TAG.INFO, string.Format("Start: Thread, {0}", this.thread.ThreadState));
            this.timer.Start();
        }

        public void Relinquish()
        {
            if (!this.isInitializedVJoy)
            {
                return;
            }

            this.timer.Stop();
            this.thread.Abort();
            this.thread.Join();
            this.logger.Add(SimpleLogger.TAG.INFO, string.Format("Abort: Thread, {0}", this.thread.ThreadState));

            this.vjoy.Relinquish();
            this.isInitializedVJoy = false;
            this.logger.Add(SimpleLogger.TAG.INFO, string.Format("Relinquished: vJoy device number {0}.", this.vjoy.id));
        }

        private void InputButton(int id)
        {
            var target = this.buttons[id];
            this.logger.Add(SimpleLogger.TAG.INFO, string.Format("Input: Button {0}, \"{1}\", {2} => {3}",
                target.id, target.pattern, target.oldValue, target.value));
            this.vjoy.InputButton(true, (uint)(target.id + 1));
        }

        private void InputAxis(int id)
        {
            var target = this.axis[id];
            this.logger.Add(SimpleLogger.TAG.INFO, string.Format("Input: Axis {0}, \"{1}\", {2} => {3}",
                target.id, target.pattern, target.oldValue, target.value));
            switch (target.id)
            {
                case 0: this.vjoy.InputAxis1Y(0); break;
                case 1: this.vjoy.InputAxis1X((int)(this.axisMaxValue - 1)); break;
                case 2: this.vjoy.InputAxis1Y((int)(this.axisMaxValue - 1)); break;
                case 3: this.vjoy.InputAxis1X(0); break;
                case 4: this.vjoy.InputAxis2Y(0); break;
                case 5: this.vjoy.InputAxis2X((int)(this.axisMaxValue - 1)); break;
                case 6: this.vjoy.InputAxis2Y((int)(this.axisMaxValue - 1)); break;
                case 7: this.vjoy.InputAxis2X(0); break;
                default: break;
            }
        }

        private void InputPOV(int id)
        {
            var target = this.pov[id];
            this.logger.Add(SimpleLogger.TAG.INFO, string.Format("Input: POV {0}, \"{1}\", {2} => {3}",
                target.id, target.pattern, target.oldValue, target.value));
            this.vjoy.InputDiscPov(target.id);
        }

        public void Update()
        {
            if (!this.isInitializedVJoy)
            {
                return;
            }

            while(true)
            {
                this.vjoy.ResetInput();
                this.vjoy.InputAxis2Y((int)(this.axisMaxValue / 2));

                string source = "";

                if (this.sourceQueue.Any())
                {
                    source = this.sourceQueue.Dequeue();
                }

                TimeSpan now = this.timer.Elapsed;
                this.buttons.Where(_ => _.enable).Where(_ => _.Updated(now, this.span, source)).Select(_ => { InputButton(_.id); return _; }).ToArray();
                this.axis.Where(_ => _.enable).Where(_ => _.Updated(now, this.span, source)).Select(_ => { InputAxis(_.id); return _; }).ToArray();
                this.pov.Where(_ => _.enable).Where(_ => _.Updated(now, this.span, source)).Select(_ => { InputPOV(_.id); return _; }).ToArray();

                Thread.Sleep(10);
            }
        }

        public void SetSource(string source)
        {
            this.sourceQueue.Enqueue(source);
        }

        public class Inputer
        {
            private Regex regex;

            public int id { get; private set; }
            public bool value { get; private set; }
            public bool oldValue { get; private set; }
            public TimeSpan time { get; private set; }
            public string pattern { get; private set; }
            public bool enable { get; private set; }

            public Inputer(int id, bool enable, TimeSpan now, string pattern)
            {
                this.id = id;
                this.enable = enable;
                this.value = false;
                this.oldValue = false;
                this.time = now;
                SetPattern(pattern);
            }

            private void SetInput(TimeSpan now)
            {
                this.oldValue = this.value;
                this.value = true;
                this.time = now;
            }

            public void SetPattern(string pattern)
            {
                this.pattern = pattern;
                this.enable = this.enable && !string.IsNullOrEmpty(this.pattern);
                this.regex = new Regex(this.pattern);
            }

            public bool Updated(TimeSpan now, TimeSpan span, string source)
            {
                if (now - this.time > span)
                {
                    this.value = false;
                }

                if (!string.IsNullOrEmpty(source) && this.regex.IsMatch(source))
                {
                    SetInput(now);
                }

                return this.value;
            }
        }

        public class SimpleQueue<T>
        {
            private Queue<T> queue;

            public SimpleQueue()
            {
                this.queue = new Queue<T>();
            }

            public bool Any()
            {
                bool result;
                lock (((ICollection)this.queue).SyncRoot)
                {
                    result = this.queue.Any();
                }
                return result;
            }

            public void Enqueue(T item)
            {
                lock (((ICollection)this.queue).SyncRoot)
                {
                    this.queue.Enqueue(item);
                }
            }

            public T Dequeue()
            {
                T item;
                lock (((ICollection)this.queue).SyncRoot)
                {
                    item = this.queue.Dequeue();
                }
                return item;
            }
        }
    }
}
