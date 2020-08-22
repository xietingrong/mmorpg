using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class UISetting:UIWindow
{
    public void ExitToCharSlect()
    {
        SceneManager.Instance.LoadScene("CharSelect");
        SoundManager.Instance.PlaySound(SoundDefine.Music_Select);
        Services.UserService.Instance.SendGameLeave();
    }
    public void SystemConfig()
    {
        UIManager.Instance.Show<UISystemConfig>();
        this.Close();
    }
    public void ExitGame()
    {
        Services.UserService.Instance.SendGameLeave(true);
    }
}


