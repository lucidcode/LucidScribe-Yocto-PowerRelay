using System;
using System.Threading;
using System.Windows.Forms;

namespace lucidcode.LucidScribe.Plugout.Yocto.PowerRelay
{
    public class PlugoutHandler : Interface.LucidPlugoutBase
    {

        private Boolean Failed = false;
        private Boolean On = false;

        private Thread SwitchOffThread;
        private Boolean SwitchingOff = false;

        private Boolean AllowToSwitchOn = true;
        private Thread AllowToSwitchBackOnThread;

        public override string Name
        {
            get { return "Yocto PowerRelay"; }
        }

        public override bool Initialize()
        {
            return true;
        }

        public override void Dispose()
        {
            return;
        }

        public override void Trigger()
        {
            try
            {
                if (Failed) return;

                if (On) return;

                if (!AllowToSwitchOn) return;

                YocoWrapper.YRelay relay;
                string errorMessage = "";

                if (YocoWrapper.YAPI.RegisterHub("usb", ref errorMessage) != YocoWrapper.YAPI.SUCCESS)
                {
                    Failed = true;
                    MessageBox.Show("RegisterHub Exception: " + errorMessage, "Yocto PowerRelay Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                relay = YocoWrapper.YRelay.FirstRelay();
                if (relay == null)
                {
                    Failed = true;
                    MessageBox.Show("No module connected (check USB cable).", "Yocto PowerRelay Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if (!relay.isOnline())
                {
                    Failed = true;
                    MessageBox.Show("tACS RegisterHub error: Module not connected (check USB cable).", "Yocto PowerRelay Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (relay.get_state() == YocoWrapper.YRelay.STATE_A)
                {
                    relay.set_state(YocoWrapper.YRelay.STATE_B);
                }
                else
                {
                    relay.set_state(YocoWrapper.YRelay.STATE_A);
                }

                On = true;

                if (SwitchingOff)
                {
                    return;
                }

                // Turn it off in 500 ms
                SwitchOffThread = new Thread(SwitchOff);
                SwitchOffThread.Start();

                // And allow it to turn back on in 1 second
                AllowToSwitchBackOnThread = new Thread(AllowToSwitchBackOn);
                AllowToSwitchBackOnThread.Start();
            }
            catch (Exception ex)
            {
                Failed = true;
                MessageBox.Show(ex.Message, "Yocto PowerRelay Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void SwitchOff()
        {
            Thread.Sleep(500);
            On = false;
            SwitchingOff = true;
            Trigger();
            SwitchingOff = false;
            On = false;
            AllowToSwitchOn = false;
        }

        public void AllowToSwitchBackOn()
        {
            Thread.Sleep(1000);
            AllowToSwitchOn = true;
        }

    }

}
