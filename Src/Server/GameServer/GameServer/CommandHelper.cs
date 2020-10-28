using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    class CommandHelper
    {
        public static void Run()
        {
            bool run = true;
            while (run)
            {
                Console.Write(">");
                string line = Console.ReadLine().ToLower().Trim();
                if(string.IsNullOrWhiteSpace(line))
                {
                    Help();
                    continue;
                }
                try
                {
                    string[] cmd = line.Split("".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    switch (cmd[0])
                    {
                        case "addexp":
                            AddExp(int.Parse(cmd[1]), int.Parse(cmd[2]));
                            break;
                        case "exit":
                            run = false;
                            break;
                        default:
                            Help();
                            break;
                    }
                }
                catch(Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                }
               
            }
        }
        public static void AddExp(int characterId,int exp)
        {
            var cha = Managers.CharacterManager.Instance.GetCharacter(characterId);
            if(cha == null)
            {
                Console.WriteLine("characterId{0} not found", characterId);
                return;
            }
            cha.AddExp(exp);
        }
        public static void Help()
        {
            Console.Write(@"
Help:
    addexp <characterId> <exp> Add exp for Character
    exit    Exit Game Server
    help    Show Help
");
        }
    }
}
