using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace BackTestingPlatform.Charts
{
    public class PythonDraw
    {
        public void draw(string path)
        {
            /*
            ScriptEngine engine = Python.CreateEngine();
            ICollection<string> Paths = engine.GetSearchPaths();
            Paths.Add("D:\\Program Files (x86)\\Anaconda3\\");
            engine.SetSearchPaths(Paths);
            ScriptScope scope = engine.CreateScope();
            ScriptSource script = engine.CreateScriptSourceFromFile("D:\\Program Files (x86))\\Projcet\\BackTestingPlatform\\BackTestingPlatform\\Charts\\ChartPython.py");
            script.Execute(scope.GetPythonChart(path));

            var result = script.Execute(scope);
            */
            //第一句代码创建了一个Python的运行环境，第二句则使用.net4.0的语法创建了一个动态的对象， OK，
            //下面就可以用这个dynamic类型的对象去调用刚才在定义的welcome方法了。
            //ScriptRuntime pyRuntime = Python.CreateRuntime();

            //var engine = pyRuntime.GetEngine("Python");

            //var paths = engine.GetSearchPaths();
            var engine = Python.CreateEngine();
            ScriptSource source = engine.CreateScriptSourceFromString("D:\\Program Files (x86))\\Projcet\\BackTestingPlatform\\BackTestingPlatform\\Charts\\ChartPython.py");
            CompiledCode compiledCode = source.Compile();

            ScriptScope scope = engine.CreateScope();
            scope.SetVariable("path", path);
            compiledCode.Execute<string>(scope);

            //paths.Add("D:\\Program Files (x86)\\Anaconda3\\");
            //paths.Add("D:\\Program Files (x86)\\Anaconda3\\Scripts\\");
            //paths.Add("D:\\Program Files (x86)\\Anaconda3\\Library\bin\\");

            //engine.SetSearchPaths(paths);
            //dynamic obj = pyRuntime.UseFile(@"D:\\Program Files (x86))\\Projcet\\BackTestingPlatform\\BackTestingPlatform\\Charts\\ChartPython.py");
            //obj.GetPythonChart(path);
        }

    }
}
