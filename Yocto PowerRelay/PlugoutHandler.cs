using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Xml;
using System.Net;

namespace lucidcode.LucidScribe.Plugout.Yocto.PowerRelay
{
  public class PlugoutHandler : lucidcode.LucidScribe.Interface.LucidPlugoutBase
  {

    private Boolean Failed = false;
    private Boolean On = false;
    private Thread SwitchOffThread;
    private Boolean SwitchingOff = false;

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

        if (relay.isOnline())
        {
          if (relay.get_state() == YocoWrapper.YRelay.STATE_A)
          {
            relay.set_state(YocoWrapper.YRelay.STATE_B);
          }
          else
          {
            relay.set_state(YocoWrapper.YRelay.STATE_A);
          }

          On = true;

          if (!SwitchingOff)
          {
            // Turn it off in a minute
            SwitchOffThread = new Thread(SwitchOff);
            SwitchOffThread.Start();
          }
        }
        else
        {
          MessageBox.Show("tACS RegisterHub error: Module not connected (check USB cable).", "Yocto PowerRelay Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
      }
      catch (Exception ex)
      {
        Failed = true;
        MessageBox.Show(ex.Message, "Yocto PowerRelay Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    public void SwitchOff()
    {
      Thread.Sleep(1000 * 60);
      On = false;
      SwitchingOff = true;
      Trigger();
      SwitchingOff = false;
      On = false;
    }


  }

}
