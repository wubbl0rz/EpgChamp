using Newtonsoft.Json;

namespace backend;

public static class Helper
{
  public static void Dump(this object o)
  {
    try
    {
      var result = o;
      if (o is Task task)
      {
        task.Wait();
        result = (object)((dynamic)task).Result;
      }
      
      Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
    }
    catch (Exception)
    {
      // ignored
    }
  }
}