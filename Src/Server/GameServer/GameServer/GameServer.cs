using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Configuration;

using System.Threading;

using Network;
using GameServer.Services;

namespace GameServer
{
    class GameServer
    {
        Thread thread;
        bool running = false;
        public bool Init()
        {
            DBService.Instance.Init();
            thread = new Thread(new ThreadStart(this.Update));
            return true;
        }

        public void Start()
        {
            running = true;
            thread.Start();
        }


        public void Stop()
        {
            running = false;
            thread.Join();
        }

        public void Update()
        {
            while (running)
            {
                Time.Tick();
                Thread.Sleep(100);
                //Console.WriteLine("{0} {1} {2} {3} {4}", Time.deltaTime, Time.frameCount, Time.ticks, Time.time, Time.realtimeSinceStartup);
            }
        }
    }
}
