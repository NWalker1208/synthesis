﻿//using System;
//using System.IO;
//using System.Text;
//using OpenTK.Graphics.OpenGL;

//namespace OGLViewer
//{
//    public class ShaderLoader
//    {
//        private static int _partShader = -1;

//        public static int PartShader
//        {
//            get
//            {
//                if (_partShader == -1)
//                {
//                    _partShader = loadPartShader();

//                    if (_partShader == -1) throw new IOException("Couldn't load shader program. See logs for more details");
//                }
//                return _partShader;
//            }
//            set
//            {
//                if (GL.IsProgram(value))
//                {
//                    GL.DeleteProgram(_partShader);
//                    _partShader = value;
//                }
//                else throw new ArgumentException("Provided program ID isn't valid");
//            }
//        }

//        private static int loadPartShader()
//        {
//#if DEBUG
//            Console.WriteLine("Compiling shader program");
//#endif

//            int shaderProgram = GL.CreateProgram();
//            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
//            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);

//            string vertexShaderSource, fragmentShaderSource;

//            try
//            {
//                //string myExeDir = (new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location)).DirectoryName;

//                //StreamReader vertexSourceReader = new StreamReader(myExeDir + "\\Shaders\\shader.vert");
//                //StreamReader fragmentSourceReader = new StreamReader(myExeDir + "\\Shaders\\shader.frag");

//                StreamReader vertexSourceReader = new StreamReader(new MemoryStream(JointResolver.Properties.Resources.vertShader));
//                StreamReader fragmentSourceReader = new StreamReader(new MemoryStream(JointResolver.Properties.Resources.fragShader));

//                vertexShaderSource = vertexSourceReader.ReadToEnd();
//                fragmentShaderSource = fragmentSourceReader.ReadToEnd();

//                vertexSourceReader.Close();
//                fragmentSourceReader.Close();
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e);
//                return -1;
//            }

//#if DEBUG
//            Console.WriteLine("Vertex shader:\n" + vertexShaderSource);
//            Console.WriteLine("Fragment shader:\n" + fragmentShaderSource);
//#endif

//            int status;

//            GL.ShaderSource(vertexShader, vertexShaderSource);
//            GL.CompileShader(vertexShader);
//            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out status);
//            if (status == 0)
//            {
//                Console.WriteLine("Vertex shader log:\n" + GL.GetShaderInfoLog(vertexShader));
//                return -1;
//            }

//            GL.ShaderSource(fragmentShader, fragmentShaderSource);
//            GL.CompileShader(fragmentShader);
//            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out status);
//            if (status == 0)
//            {
//                Console.WriteLine("Fragment shader log:\n" + GL.GetShaderInfoLog(fragmentShader));
//                return -1;
//            }

//            GL.AttachShader(shaderProgram, vertexShader);
//            GL.AttachShader(shaderProgram, fragmentShader);
//            GL.LinkProgram(shaderProgram);
//            GL.GetProgram(shaderProgram, GetProgramParameterName.LinkStatus, out status);
//            if (status == 0)
//            {
//                Console.WriteLine("Shader program log:\n" + GL.GetProgramInfoLog(shaderProgram));
//                return -1;
//            }

//#if DEBUG
//            Console.WriteLine("Shaders compiled and linked successfully");
//#endif

//            return shaderProgram;
//        }
//    }
//}