using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Knock
{
  
  public enum MessageSide
  {
    Me,
    You
  }

  /// <summary>
  /// Comments.
  /// </summary>
  public class Message
  {
    
    public Message()
    {
     
    }

    public string Text { get; set; }

    public DateTime Timestamp { get; set; }

    public MessageSide Side { get; set; }
  }
}
