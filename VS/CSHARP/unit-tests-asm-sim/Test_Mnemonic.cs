﻿using System;
using System.Collections.Generic;
using System.Numerics; // for BigInt

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Z3;

using AsmSim;
using AsmTools;
using AsmSim.Mnemonics;

namespace unit_tests_asm_z3
{
    [TestClass]
    public class Test_Mnemonic
    {
        const bool logToDisplay = TestTools.LOG_TO_DISPLAY;

        public Test_Mnemonic()
        {
            Console.WriteLine("logToDisplay=" + logToDisplay);
        }

        private Tools CreateTools(int timeOut = TestTools.DEFAULT_TIMEOUT)
        {
            /* The following parameters can be set: 
                    - proof (Boolean) Enable proof generation
                    - debug_ref_count (Boolean) Enable debug support for Z3_ast reference counting
                    - trace (Boolean) Tracing support for VCC 
                    - trace_file_name (String) Trace out file for VCC traces 
                    - timeout (unsigned) default timeout (in milliseconds) used for solvers 
                    - well_sorted_check type checker 
                    - auto_config use heuristics to automatically select solver and configure it 
                    - model model generation for solvers, this parameter can be overwritten when creating a solver 
                    - model_validate validate models produced by solvers 
                    - unsat_core unsat-core generation for solvers, this parameter can be overwritten when creating 
                            a solver Note that in previous versions of Z3, this constructor was also used to set 
                            global and module parameters. For this purpose we should now use 
                            Microsoft.Z3.Global.SetParameter(System.String,System.String)
            */

            Dictionary<string, string> settings = new Dictionary<string, string>
            {
                { "unsat_core", "false" },    // enable generation of unsat cores
                { "model", "false" },         // enable model generation
                { "proof", "false" },         // enable proof generation
                { "timeout", timeOut.ToString() }
            };
            return new Tools(settings);
        }

        private State CreateState(Tools tools)
        {
            string tailKey = "!ROOT";// Tools.CreateKey(tools.Rand);
            string headKey = tailKey;
            return new State(tools, tailKey, headKey);
        }

        #region MOV reg

        [TestMethod]
        public void Test_MnemonicZ3_Mov_reg1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RCX = true;

            ulong value_rcx = 10;
            string line1 = "mov rcx, " + value_rcx;
            string line2 = "mov rbx, rcx";
            string line3 = "mov rax, rbx";

            if (true)
            {
                if (logToDisplay) Console.WriteLine("Forward:");
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, value_rcx, state);
                TestTools.AreEqual(Rn.RBX, value_rcx, state);
                TestTools.AreEqual(Rn.RCX, value_rcx, state);
            }
            if (true)
            {
                if (logToDisplay) Console.WriteLine("Backward:");
                State state = CreateState(tools);
                state = Runner.SimpleStep_Backward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, value_rcx, state);
                TestTools.AreEqual(Rn.RBX, value_rcx, state);
                TestTools.AreEqual(Rn.RCX, value_rcx, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Mov_reg2()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RCX = true;

            string line1 = "mov rbx, rax";
            string line2 = "mov rcx, rbx";

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, Rn.RCX, state);
            }
            {   // backward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Backward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, Rn.RCX, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Mov_reg3()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            ulong value_eax = 10;
            string line1 = "mov eax, " + value_eax;
            string line2 = "mov ebx, eax";
            ulong value_eax_2 = 5;
            string line3 = "mov eax, " + value_eax_2;
            {
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.EAX, value_eax, state);
                TestTools.AreEqual(Rn.RBX, "????????.????????.????????.????????.????????.????????.????????.????????", state);

                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.EAX, value_eax, state);
                TestTools.AreEqual(Rn.EBX, value_eax, state);

                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.EAX, value_eax_2, state);
                TestTools.AreEqual(Rn.EBX, value_eax, state);
            }
            {
                State state = CreateState(tools);

                state = Runner.SimpleStep_Backward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Backward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Backward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.EAX, value_eax_2, state);
                TestTools.AreEqual(Rn.EBX, value_eax, state);
            }
        }

        #endregion
        #region MOV mem

        [TestMethod]
        public void Test_MnemonicZ3_Mov_mem1_1byte_a()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RCX = true;
            tools.StateConfig.mem = true;

            string line1 = "mov byte ptr [rax], bl";
            string line2 = "mov cl, byte ptr [rax]";

            if (true)
            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("Forward: After \"" + line1 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("Forward: After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.BL, Rn.CL, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Mov_mem1_1byte_b()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RCX = true;
            tools.StateConfig.RDX = true;
            tools.StateConfig.mem = true;

            string line1 = "mov byte ptr [rax], bl";
            string line2 = "mov cl, byte ptr [rax]";
            string line3 = "mov byte ptr [rax], dl";
            string line4 = "mov cl, byte ptr [rax]";

            if (true) {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("Forward: After \"" + line1 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("Forward: After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.BL, Rn.CL, state);

                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("Forward: After \"" + line3 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Forward(line4, state);
                if (logToDisplay) Console.WriteLine("Forward: After \"" + line4 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.DL, Rn.CL, state);
            }
            if (true) {   // backward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Backward(line2, state);
                if (logToDisplay) Console.WriteLine("Backward: After \"" + line2 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line1, state);
                if (logToDisplay) Console.WriteLine("Backward: After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.BL, Rn.CL, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Mov_mem1_2byte()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RCX = true;
            tools.StateConfig.mem = true;

            string line1 = "mov word ptr [rax], bx";
            string line2 = "mov cx, word ptr [rax]";
            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("Forward: After \"" + line1 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("Forward: After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.BX, Rn.CX, state);
            }
            {   // backward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Backward(line2, state);
                if (logToDisplay) Console.WriteLine("Backward: After \"" + line2 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line1, state);
                if (logToDisplay) Console.WriteLine("Backward: After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.BX, Rn.CX, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Mov_mem1_4byte()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RCX = true;
            tools.StateConfig.mem = true;

            string line1 = "mov dword ptr [rax], ebx";
            string line2 = "mov ecx, dword ptr [rax]";
            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("Forward: After \"" + line1 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("Forward: After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.EBX, Rn.ECX, state);
            }
            {   // backward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Backward(line2, state);
                if (logToDisplay) Console.WriteLine("Backward: After \"" + line2 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line1, state);
                if (logToDisplay) Console.WriteLine("Backward: After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.EBX, Rn.ECX, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Mov_mem1_8byte()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RCX = true;
            tools.StateConfig.mem = true;

            string line1 = "mov qword ptr [rax], rbx";
            string line2 = "mov rcx, qword ptr [rax]";
            if (false) {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("Forward: After \"" + line1 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("Forward: After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RBX, Rn.RCX, state);
            }
            {   // backward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Backward(line2, state);
                if (logToDisplay) Console.WriteLine("Backward: After \"" + line2 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line1, state);
                if (logToDisplay) Console.WriteLine("Backward: After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RBX, Rn.RCX, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Mov_mem2()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RCX = true;
            tools.StateConfig.mem = true;

            string line1 = "mov rbx, rax";
            string line2 = "mov rax, qword ptr [rcx + 2 * rax]";
            string line3 = "mov rbx, qword ptr [rcx + 2 * rbx]";
            string line4 = "xor rax, rbx";
            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line4, state);
                if (logToDisplay) Console.WriteLine("After \"" + line4 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, 0, state);
            }
            {   // backward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Backward(line4, state);
                if (logToDisplay) Console.WriteLine("After \"" + line4 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, 0, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Mov_mem3()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.ZF = true;
            tools.StateConfig.mem = true;

            ulong address = 10;
            ulong value = 20;
            string line1 = "mov qword ptr [" + address + "], " + value;
            string line2 = "mov rax, qword ptr [rbx]";
            string line3 = "cmp rbx, " + address;
            string line4 = "jnz label";

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                state.Add(new BranchInfo(ToolsAsmSim.ConditionalTaken(ConditionalElement.NZ, state.HeadKey, state.Ctx), false));
                if (logToDisplay) Console.WriteLine("After \"" + line4 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, value, state);
                TestTools.AreEqual(Rn.RBX, address, state);
            }
            {   // backward
                State state = CreateState(tools);
                state.Add(new BranchInfo(ToolsAsmSim.ConditionalTaken(ConditionalElement.NZ, state.HeadKey, state.Ctx), false));
                if (logToDisplay) Console.WriteLine("After \"" + line4 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, value, state);
                TestTools.AreEqual(Rn.RBX, address, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Mov_mem4()
        {
            // Test Memory overwrite

            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RCX = true;
            tools.StateConfig.RDX = true;
            tools.StateConfig.mem = true;

            string line1 = "mov qword ptr [rax], rbx";
            string line2 = "mov rdx, qword ptr [rax]";
            string line3 = "mov qword ptr [rax], rcx";
            string line4 = "mov rdx, qword ptr [rax]";

            if (true) {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RDX, Rn.RBX, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line4, state);
                if (logToDisplay) Console.WriteLine("After \"" + line4 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RDX, Rn.RCX, state);
            }
            if (false) {   // backward: TODO is this test correct??
                State state = CreateState(tools);
                state = Runner.SimpleStep_Backward(line4, state);
                if (logToDisplay) Console.WriteLine("After \"" + line4 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RDX, Rn.RCX, state);
                state = Runner.SimpleStep_Backward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RDX, Rn.RCX, state);// rbx is written to [rax] before [rax] is written with rcx, hence rcx is equal to rdx
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Mov_mem5()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RCX = true;
            tools.StateConfig.ZF = true;
            tools.StateConfig.mem = true;

            // test: chaining memory dereference with branch constraints afterwards

            string line1 = "mov qword ptr [10], 20";
            string line2 = "mov qword ptr [20], 30";
            string line3 = "mov rbx, qword ptr [rcx]";
            string line4 = "mov rax, qword ptr [rbx]";
            string line5 = "cmp rcx, 10";
            string line6 = "jnz label";

            if (true) {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                //if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line3, state);
                //if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line4, state);
                //if (logToDisplay) Console.WriteLine("After \"" + line4 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line5, state);
                //if (logToDisplay) Console.WriteLine("After \"" + line5 + "\", we know:\n" + state);
                state.Add(new BranchInfo(ToolsAsmSim.ConditionalTaken(ConditionalElement.NZ, state.HeadKey, state.Ctx), false));
                //if (logToDisplay) Console.WriteLine("After \"" + line6 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, 30, state);
                TestTools.AreEqual(Rn.RBX, 20, state);
                TestTools.AreEqual(Rn.RCX, 10, state);
            }
            else Assert.Inconclusive("SLOW");

            if (false) // is this test correct??
            {   // backward
                State state = CreateState(tools);
                state.Add(new BranchInfo(ToolsAsmSim.ConditionalTaken(ConditionalElement.NZ, state.HeadKey, state.Ctx), false));
                if (logToDisplay) Console.WriteLine("After \"" + line6 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line5, state);
                if (logToDisplay) Console.WriteLine("After \"" + line5 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line4, state);
                if (logToDisplay) Console.WriteLine("After \"" + line4 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line1, state);

                TestTools.AreEqual(Rn.RAX, 30, state);
                TestTools.AreEqual(Rn.RBX, 20, state);
                TestTools.AreEqual(Rn.RCX, 10, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Mov_mem6()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RCX = true;
            tools.StateConfig.mem = true;

            // test dword write read
            string line1 = "mov dword ptr [rax], ebx";
            string line2 = "mov ecx, dword ptr [rax]";
            if (true) {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("Forward: After \"" + line1 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("Forward: After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.EBX, Rn.ECX, state);
            }
            if (true)
            {   // backward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Backward(line2, state);
                if (logToDisplay) Console.WriteLine("Backward: After \"" + line2 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line1, state);
                if (logToDisplay) Console.WriteLine("Backward: After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.EBX, Rn.ECX, state);
            }
        }
        #endregion
        #region LEA

        [TestMethod]
        public void Test_MnemonicZ3_Lea_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RCX = true;
            tools.StateConfig.RDX = true;
            tools.StateConfig.mem = true;

            string line1 = "mov rdx, rcx";
            string line2 = "lea rax, byte ptr [rcx]";
            string line3 = "lea rbx, byte ptr [rdx]";
            if (true)
            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                //if (logToDisplay) Console.WriteLine("Forward: After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RDX, Rn.RCX, state);

                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("Forward: After \"" + line2 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("Forward: After \"" + line3 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, Rn.RBX, state);
            }
            if (true)
            {   // backward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Backward(line3, state);
                if (logToDisplay) Console.WriteLine("Backward: After \"" + line3 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Backward(line2, state);
                if (logToDisplay) Console.WriteLine("Backward: After \"" + line2 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Backward(line1, state);
                if (logToDisplay) Console.WriteLine("Backward: After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, Rn.RBX, state);
                TestTools.AreEqual(Rn.RDX, Rn.RCX, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Lea_2()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RCX = true;
            tools.StateConfig.RDX = true;
            tools.StateConfig.mem = true;

            string line1 = "mov rdx, rcx";
            string line2 = "lea rax, byte ptr [2 * rcx + 10]";
            string line3 = "lea rbx, byte ptr [2 * rdx + 10]";
            if (true)
            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                //if (logToDisplay) Console.WriteLine("Forward: After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RDX, Rn.RCX, state);

                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("Forward: After \"" + line2 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("Forward: After \"" + line3 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, Rn.RBX, state);
            }
            if (true)
            {   // backward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Backward(line3, state);
                if (logToDisplay) Console.WriteLine("Backward: After \"" + line3 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Backward(line2, state);
                if (logToDisplay) Console.WriteLine("Backward: After \"" + line2 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Backward(line1, state);
                if (logToDisplay) Console.WriteLine("Backward: After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, Rn.RBX, state);
                TestTools.AreEqual(Rn.RDX, Rn.RCX, state);
            }
        }

        #endregion

        [TestMethod]
        public void Test_MnemonicZ3_Add_Backwards()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            string line1 = "mov rax, 10";
            string line2 = "mov rbx, 20";
            string line3 = "add rax, rbx";
            { // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, 30, state);
                TestTools.AreEqual(Rn.RBX, 20, state);
            }
            { // backward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Backward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, 30, state);
                TestTools.AreEqual(Rn.RBX, 20, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Cmovcc_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.ZF = true;

            ulong value_rax = 10;
            ulong value_rbx = 20;

            string line1 = "mov rax, " + value_rax;
            string line2 = "mov rbx, " + value_rbx;
            string line3 = "cmovz rbx, rax";
            string line4 = "jz label1";

            State state = CreateState(tools);

            {
                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, value_rax, state);
                TestTools.AreEqual(Rn.RBX, value_rbx, state);
                state = Runner.SimpleStep_Forward(line3, state);

                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, value_rax, state);
                TestTools.AreEqual(Rn.RBX, "00000000.00000000.00000000.00000000.00000000.00000000.00000000.000????0", state);
            }
            {
                var tup = Runner.Step_Forward(line4, state);
                State state1 = tup.Regular;
                State state2 = tup.Branch;

                if (logToDisplay) Console.WriteLine("After \"" + line4 + "\", Branch NOT taken, we know:\n" + state1);
                TestTools.AreEqual(Rn.RAX, value_rax, state1);
                TestTools.AreEqual(Rn.RBX, value_rbx, state1);

                if (logToDisplay) Console.WriteLine("After \"" + line4 + "\", Branch taken, we know:\n" + state2);
                TestTools.AreEqual(Rn.RAX, value_rax, state2);
                TestTools.AreEqual(Rn.RBX, value_rax, state2);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Cmovcc_2()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.ZF = true;

            string line1 = "cmovz rbx, rax";

            State state = CreateState(tools);
            Context ctx = state.Ctx;
            {
                StateUpdate updateState = new StateUpdate("!PREVKEY", "!NEXTKEY", state.Tools);
                updateState.Set(Rn.RAX, "00000000_00000000_00000000_00000000_00000000_00000000_00000000_0000000U");
                updateState.Set(Rn.RBX, "00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000010");
                state.Update_Forward(updateState);

                if (logToDisplay) Console.WriteLine("Before we know:\n" + state);

                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RBX, "00000000.00000000.00000000.00000000.00000000.00000000.00000000.000000?U", state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Sub_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            ulong value_rax = 0x0AAAAAAAAAAAAAAA;
            ulong value_rbx = 0x00BBBBBBBBBBBBBB;
            ulong value_rax_2 = value_rax - value_rbx;

            string line1 = "mov rax, " + value_rax;
            string line2 = "mov rbx, " + value_rbx;
            string line3 = "sub rax, rbx";

            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, value_rax, state);
                TestTools.AreEqual(Rn.RBX, "????????.????????.????????.????????.????????.????????.????????.????????", state);

                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, value_rax, state);
                TestTools.AreEqual(Rn.RBX, value_rbx, state);

                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, value_rax_2, state);
                TestTools.AreEqual(Rn.RBX, value_rbx, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Sub_2()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.OF = true;

            uint nBits = 8;
            string line1 = "sub al, bl";

            {   // forward
                State state = CreateState(tools);

                BitVecExpr al0 = state.Create(Rn.AL);
                BitVecExpr bl0 = state.Create(Rn.BL);

                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);

                BoolExpr overflowExpr = ToolsFlags.Create_OF_Sub(al0.Translate(state.Ctx) as BitVecExpr, bl0.Translate(state.Ctx) as BitVecExpr, nBits, state.Ctx);
                BoolExpr eq = state.Ctx.MkEq(state.Create(Flags.OF), overflowExpr);
                Assert.AreEqual(Tv.ONE, ToolsZ3.GetTv(eq, state.Solver, state.Ctx));
            }
        }
        [TestMethod]
        public void Test_MnemonicZ3_Add_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.OF = true;

            
            uint nBits = 8;
            string line1 = "add al, bl";
            {
                ulong a = 0b0000_1000;
                ulong b = 0b0000_0100;

                State state = CreateState(tools);
                Context ctx = state.Ctx;

                StateUpdate updateState = new StateUpdate(state.TailKey, Tools.CreateKey(state.Tools.Rand), tools);
                if (logToDisplay) Console.WriteLine("Intially, we know:\n" + state);

                updateState.Set(Rn.AL, a);
                updateState.Set(Rn.BL, b);

                state.Update_Forward(updateState);
                if (logToDisplay) Console.WriteLine("Before \"" + line1 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);

                TestTools.AreEqual(Flags.OF, false, state);
                TestTools.AreEqual(Rn.AL, a + b, state);
            }
            {
                ulong a = 0b1100_0000;
                ulong b = 0b1000_0000;

                State state = CreateState(tools);
                Context ctx = state.Ctx;

                StateUpdate updateState = new StateUpdate("!PREVKEY", "!NEXTKEY", tools);
                updateState.Set(Rn.AL, a);
                updateState.Set(Rn.BL, b);
                state.Update_Forward(updateState);

                state = Runner.SimpleStep_Forward(line1, state);
                //if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);

                BoolExpr of = ToolsFlags.Create_OF_Add(state.Ctx.MkBV(a, nBits), state.Ctx.MkBV(b, nBits), nBits, state.Ctx);
                if (logToDisplay) Console.WriteLine(of);
                TestTools.AreEqual(Flags.OF, true, state);
                TestTools.AreEqual(Rn.AL, a + b, state); //NOTE: only the lower 8bits are checked for equality!
            }
            {
                ulong a = 0b0000_1000;
                ulong b = 0b0000_0100;

                State state = CreateState(tools);
                Context ctx = state.Ctx;

                StateUpdate updateState = new StateUpdate("!PREVKEY", "!NEXTKEY", tools);
                updateState.Set(Rn.AL, a);
                updateState.Set(Rn.BL, b);
                state.Update_Forward(updateState);


                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);

                BoolExpr of = ToolsFlags.Create_OF_Add(state.Ctx.MkBV(a, nBits), state.Ctx.MkBV(b, nBits), nBits, state.Ctx);
                if (logToDisplay) Console.WriteLine(of);
                TestTools.AreEqual(Flags.OF, false, state);
                TestTools.AreEqual(Rn.AL, a + b, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Add_2()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            int nExperiments = 5;
            Random rand = new Random((int)DateTime.Now.Ticks);
            {
                uint nBits = 64;
                for (int i = 0; i < nExperiments; ++i)
                {
                    ulong rax_value = TestTools.RandUlong((int)nBits, rand);
                    ulong rbx_value = TestTools.RandUlong((int)nBits, rand);
                    ulong result = (rax_value + rbx_value);

                    State state = CreateState(tools);
                    Context ctx = state.Ctx;

                    StateUpdate updateState = new StateUpdate("!PREVKEY", "!NEXTKEY", state.Tools);
                    updateState.Set(Rn.RAX, rax_value);
                    updateState.Set(Rn.RBX, rbx_value);
                    state.Update_Forward(updateState);

                    string line = "add rax, rbx";
                    state = Runner.SimpleStep_Forward(line, state);

                    if (logToDisplay)
                        Console.WriteLine("After \"" + line + "\", we know:\n" + state);

                    TestTools.AreEqual(Rn.RAX, result, state);
                    TestTools.AreEqual(Flags.CF, TestTools.ToTv5(TestTools.Calc_CF_Add(nBits, rax_value, rbx_value)), state);
                    TestTools.AreEqual(Flags.OF, TestTools.ToTv5(TestTools.Calc_OF_Add(nBits, rax_value, rbx_value)), state);
                    TestTools.AreEqual(Flags.AF, TestTools.ToTv5(TestTools.Calc_AF_Add(rax_value, rbx_value)), state);
                    TestTools.AreEqual(Flags.PF, TestTools.ToTv5(TestTools.Calc_PF(result)), state);
                    TestTools.AreEqual(Flags.ZF, TestTools.ToTv5(TestTools.Calc_ZF(result)), state);
                    TestTools.AreEqual(Flags.SF, TestTools.ToTv5(TestTools.Calc_SF(nBits, result)), state);
                }
            }
            {
                uint nBits = 16;
                for (int i = 0; i < nExperiments; ++i)
                {
                    ulong ax_value = TestTools.RandUlong((int)nBits, rand);
                    ulong bx_value = TestTools.RandUlong((int)nBits, rand);
                    ulong result = (ax_value + bx_value) & 0xFFFF;

                    State state = CreateState(tools);
                    Context ctx = state.Ctx;

                    StateUpdate updateState = new StateUpdate("!PREVKEY", "!NEXTKEY", state.Tools);
                    updateState.Set(Rn.AX, ax_value);
                    updateState.Set(Rn.BX, bx_value);
                    state.Update_Forward(updateState);

                    string line = "add ax, bx";
                    state = Runner.SimpleStep_Forward(line, state);

                    //if (logToDisplay) Console.WriteLine("After \"" + line + "\", we know:\n" + state);

                    TestTools.AreEqual(Rn.AX, result, state);
                    Assert.AreEqual(TestTools.ToTv5(TestTools.Calc_CF_Add(nBits, ax_value, bx_value)), TestTools.GetTv5(Flags.CF, state), "CF: ax=" + ax_value + "; bx=" + bx_value);
                    Assert.AreEqual(TestTools.ToTv5(TestTools.Calc_OF_Add(nBits, ax_value, bx_value)), TestTools.GetTv5(Flags.OF, state), "OF: ax=" + ax_value + "; bx=" + bx_value);
                    Assert.AreEqual(TestTools.ToTv5(TestTools.Calc_AF_Add(ax_value, bx_value)), TestTools.GetTv5(Flags.AF, state), "AF: ax=" + ax_value + "; bx=" + bx_value);
                    Assert.AreEqual(TestTools.ToTv5(TestTools.Calc_PF(result)), TestTools.GetTv5(Flags.PF, state), "PF: ax=" + ax_value + "; bx=" + bx_value);
                    Assert.AreEqual(TestTools.ToTv5(TestTools.Calc_ZF(result)), TestTools.GetTv5(Flags.ZF, state), "ZF: ax=" + ax_value + "; bx=" + bx_value);
                    Assert.AreEqual(TestTools.ToTv5(TestTools.Calc_SF(nBits, result)), TestTools.GetTv5(Flags.SF, state), "SF: ax=" + ax_value + "; bx=" + bx_value);
                }
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Inc_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;

            State state = CreateState(tools);

            ulong value_rax = 10;
            string line1 = "mov rax, " + value_rax;
            string line2 = "inc rax";

            {   // forward
                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, value_rax, state);

                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, value_rax + 1, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Dec_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
 
            ulong value_rax = 10;

            string line1 = "mov rax, " + value_rax;
            string line2 = "dec rax";

            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, value_rax, state);

                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, value_rax - 1, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Neg_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;

            ulong value_rax = 10;
            string line1 = "mov rax, " + value_rax;
            string line2 = "neg rax";

            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, value_rax, state);

                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, 0 - value_rax, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Xor_1_Forward()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            Random rand = new Random((int)DateTime.Now.Ticks);

            for (int i = 0; i < 1; ++i)
            {
                ulong value_rax = ToolsZ3.GetRandomUlong(rand);
                ulong value_rbx = ToolsZ3.GetRandomUlong(rand);
                ulong value_result = value_rax ^ value_rbx;

                string line1 = "mov rax, " + value_rax;
                string line2 = "mov rbx, " + value_rbx;
                string line3 = "xor rax, rbx";

                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After line 3 with \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, value_result, state);

                TestTools.AreEqual(Flags.SF, TestTools.Calc_SF(64, value_result), state);
                TestTools.AreEqual(Flags.ZF, TestTools.Calc_ZF(value_result), state);
                TestTools.AreEqual(Flags.PF, TestTools.Calc_PF(value_result), state);
                TestTools.AreEqual(Flags.OF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.CF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.AF, Tv.UNDEFINED, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Xor_1_Backward()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            Random rand = new Random((int)DateTime.Now.Ticks);

            for (int i = 0; i < 1; ++i)
            {
                ulong value_rax = ToolsZ3.GetRandomUlong(rand);
                ulong value_rbx = ToolsZ3.GetRandomUlong(rand);
                ulong value_result = value_rax ^ value_rbx;

                string line1 = "mov rax, " + value_rax;
                string line2 = "mov rbx, " + value_rbx;
                string line3 = "xor rax, rbx";

                State state = CreateState(tools);
                if (logToDisplay) Console.WriteLine("Before line 3 with \"" + line3 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line3, state);
                if (logToDisplay) Console.WriteLine("After line 3 with \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Flags.OF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.CF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.AF, Tv.UNDEFINED, state);

                state = Runner.SimpleStep_Backward(line2, state);
                if (logToDisplay) Console.WriteLine("After line 2 with \"" + line2 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Backward(line1, state);
                if (logToDisplay) Console.WriteLine("After line 1 with \"" + line1 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, value_result, state);

                TestTools.AreEqual(Flags.SF, TestTools.Calc_SF(64, value_result), state);
                TestTools.AreEqual(Flags.ZF, TestTools.Calc_ZF(value_result), state);
                TestTools.AreEqual(Flags.PF, TestTools.Calc_PF(value_result), state);
                TestTools.AreEqual(Flags.OF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.CF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.AF, Tv.UNDEFINED, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Cmp_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RCX = true;
            tools.StateConfig.ZF = true;

            State state = CreateState(tools);

            ulong value_rax = 0x0AAAAAAAAAAAAAAA;
            {
                string line = "mov rax, " + value_rax;
                state = Runner.SimpleStep_Forward(line, state);
            }
            ulong value_rbx = 0x00BBBBBBBBBBBBBB;
            {
                string line = "mov rbx, " + value_rbx;
                state = Runner.SimpleStep_Forward(line, state);
            }
            {
                string line = "cmp rax, rbx";
                state = Runner.SimpleStep_Forward(line, state);
                if (logToDisplay) Console.WriteLine("After \"" + line + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, value_rax, state);
                TestTools.AreEqual(Rn.RBX, value_rbx, state);
            }
            {
                string line = "cmove rcx, rbx";
                state = Runner.SimpleStep_Forward(line, state);
                if (logToDisplay) Console.WriteLine("After \"" + line + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, value_rax, state);
                TestTools.AreEqual(Rn.RBX, value_rbx, state);
                TestTools.AreEqual(Rn.RCX, "????????.????????.????????.????????.????????.????????.????????.????????", state);
            }
        }
        [TestMethod]
        public void Test_MnemonicZ3_Cmp_2()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.mem = true;

            State state = CreateState(tools);

            ulong value1 = 0xFF;
            ulong value2 = 0x3F;
            ulong value_result = value1 - value2;
            {
                string line = "mov byte [rax], " + value1;
                state = Runner.SimpleStep_Forward(line, state);
            }
            {
                string line = "mov bl, " + value2;
                state = Runner.SimpleStep_Forward(line, state);
            }
            {
                string line = "cmp byte [rax], bl";
                state = Runner.SimpleStep_Forward(line, state);
                if (logToDisplay) Console.WriteLine("After \"" + line + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, "????????.????????.????????.????????.????????.????????.????????.????????", state);
                TestTools.AreEqual(Rn.BL, value2, state);

                TestTools.AreEqual(Flags.SF, TestTools.Calc_SF(8, value_result), state);
                TestTools.AreEqual(Flags.ZF, TestTools.Calc_ZF(value_result), state);
                TestTools.AreEqual(Flags.PF, TestTools.Calc_PF(value_result), state);
                TestTools.AreEqual(Flags.OF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.CF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.AF, Tv.ZERO, state);
            }
        }
        #region BTS
        [TestMethod]
        public void Test_MnemonicZ3_Bts_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RCX = true;

            ulong value_rax = 0xFF00;
            ulong value_cl = 0x4;
            ulong value2_rax = value_rax | (1UL << (int)value_cl);

            string line1 = "mov rax, " + value_rax;
            string line2 = "mov cl, " + value_cl;
            string line3 = "bts rax, rcx";
            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, value_rax, state);

                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.CL, value_cl, state);

                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, value2_rax, state);
            }
            {   // backward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Backward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Backward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.CL, value_cl, state);

                state = Runner.SimpleStep_Backward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, value2_rax, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Bts_2()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RCX = true;
 
            ulong value_eax = 0xFF00;
            ulong value_cl = 0x4;
            ulong value2_eax = value_eax | (1UL << (int)value_cl);

            string line1 = "mov eax, " + value_eax;
            string line2 = "mov cl, " + value_cl;
            string line3 = "bts eax, ecx";

            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.CL, value_cl, state);

                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.EAX, value2_eax, state);
            }
            {   // backward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Backward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Backward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.CL, value_cl, state);

                state = Runner.SimpleStep_Backward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.EAX, value2_eax, state);
            }
        }
        #endregion
        #region BTC
        [TestMethod]
        public void Test_MnemonicZ3_Btc_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RCX = true;
            tools.StateConfig.CF = true;

            string line1 = "mov rax, rbx";
            string line2 = "btc rax, rcx";
            string line3 = "btc rbx, rcx";
            string line4 = "xor rax, rbx";

            if (true)
            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                //TestTools.test(Rn.RAX, "????????.????????.????????.????????.????????.????????.????????.????????", state);

                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                //TestTools.test(Rn.RAX, "????????.????????.????????.????????.????????.????????.????????.????????", state);

                state = Runner.SimpleStep_Forward(line4, state);
                if (logToDisplay) Console.WriteLine("After \"" + line4 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, 0, state);
            }
            if (true)
            {   // backward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Backward(line4, state);
                if (logToDisplay) Console.WriteLine("After \"" + line4 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Backward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                //TestTools.test(Rn.RAX, "????????.????????.????????.????????.????????.????????.????????.????????", state);

                state = Runner.SimpleStep_Backward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                //TestTools.test(Rn.RAX, "????????.????????.????????.????????.????????.????????.????????.????????", state);

                state = Runner.SimpleStep_Backward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, 0, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Btc_2()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RCX = true;
            tools.StateConfig.CF = true;

            string line1 = "mov rax, rbx";
            string line2 = "btc rax, rcx";
            string line3 = "xor rax, rbx";

            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, "????????.????????.????????.????????.????????.????????.????????.????????", state);

                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, "????????.????????.????????.????????.????????.????????.????????.????????", state);
            }
            {   // backward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, "????????.????????.????????.????????.????????.????????.????????.????????", state);

                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, "????????.????????.????????.????????.????????.????????.????????.????????", state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Btc_3()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RCX = true;
            tools.StateConfig.RDX = true;
            tools.StateConfig.CF = true;

            string line1 = "mov rdx, rcx";
            string line2 = "mov rbx, rax";
            string line3 = "btc rax, rcx";
            string line4 = "btc rbx, rdx";
            string line5 = "xor rax, rbx";
            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, "????????.????????.????????.????????.????????.????????.????????.????????", state);

                state = Runner.SimpleStep_Forward(line4, state);
                if (logToDisplay) Console.WriteLine("After \"" + line4 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RBX, "????????.????????.????????.????????.????????.????????.????????.????????", state);

                state = Runner.SimpleStep_Forward(line5, state);
                if (logToDisplay) Console.WriteLine("After \"" + line5 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, 0, state);
            }
        }

        #endregion
        #region RCL
        [TestMethod]
        public void Test_MnemonicZ3_Rcl_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.CF = true;

            string line1 = "bsf ax, ax"; // set the carry to UNDEFINED
            string line2 = "mov ax, 0";
            string line3 = "rcl eax, 1";

            if (true)
            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Flags.CF, Tv.UNDEFINED, state);

                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.AX, 0, state);

                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, "00000000.00000000.00000000.00000000.????????.???????0.00000000.0000000U", state);
            }
        }

        #endregion
        #region RCR
        [TestMethod]
        public void Test_MnemonicZ3_Rcr_3()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RCX = true;
            tools.StateConfig.CF = true;

            string line1 = "rcr rax, cl";
            string line2 = "rcl rax, cl";
            string line3 = "xor rax, rax";

            if (true) {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                //if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, "????????.????????.????????.????????.????????.????????.????????.????????", state);

                state = Runner.SimpleStep_Forward(line2, state);
                // (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, "????????.????????.????????.????????.????????.????????.????????.????????", state);

                state = Runner.SimpleStep_Forward(line3, state);
                //if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, 0, state);
            }
            if (false) // incorrect test... 
            {   // backward is 
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, 0, state);

                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, 0, state);

                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, 0, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Rcr_4()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.CF = true;

            string line1 = "rcr rax, 4";
            string line2 = "mov rbx, rax";
            string line3 = "rcl rbx, 4";
            string line4 = "rcl rax, 4";
            string line5 = "xor rax, rbx";

            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, "????????.????????.????????.????????.????????.????????.????????.????????", state);
                //TestTools.Test(Rn.RBX, "????????.????????.????????.????????.????????.????????.????????.????????", state);

                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, "????????.????????.????????.????????.????????.????????.????????.????????", state);
                TestTools.AreEqual(Rn.RBX, "????????.????????.????????.????????.????????.????????.????????.????????", state);

                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                //TestTools.Test(Rn.RAX, "????????.????????.????????.????????.????????.????????.????????.????????", state);
                TestTools.AreEqual(Rn.RBX, "????????.????????.????????.????????.????????.????????.????????.????????", state);

                state = Runner.SimpleStep_Forward(line4, state);
                if (logToDisplay) Console.WriteLine("After \"" + line4 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, "????????.????????.????????.????????.????????.????????.????????.????????", state);
                //TestTools.Test(Rn.RBX, "????????.????????.????????.????????.????????.????????.????????.????????", state);

                state = Runner.SimpleStep_Forward(line5, state);
                if (logToDisplay) Console.WriteLine("After \"" + line5 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, "00000000.00000000.00000000.00000000.00000000.00000000.00000000.0000?000", state);
                //TestTools.Test(Rn.RBX, "????????.????????.????????.????????.????????.????????.????????.????????", state);
            }
        }
        #endregion
        #region BSF
        [TestMethod]
        public void Test_MnemonicZ3_Bsf_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            string line1 = "mov rbx, 00010000_00000000_00000000_00000000_00000000_00000000_00000000_00001110b";
            string line2 = "bsf rax, rbx";

            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                //if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, 1, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Bsf_2()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            string line1 = "mov rbx, 0";
            string line2 = "bsf rax, rbx";

            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                //if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, "UUUUUUUU_UUUUUUUU_UUUUUUUU_UUUUUUUU_UUUUUUUU_UUUUUUUU_UUUUUUUU_UUUUUUUU", state);
            }
        }
        #endregion
        #region BSR
        [TestMethod]
        public void Test_MnemonicZ3_Bsr_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            string line1 = "mov rbx, 00011000_00000000_00000000_00000000_00000000_00000000_00000000_00001110b";
            string line2 = "bsr rax, rbx";

            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                //if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, 60, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Bsr_2()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            string line1 = "mov rbx, 0";
            string line2 = "bsr rax, rbx";

            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                //if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);

                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, "UUUUUUUU_UUUUUUUU_UUUUUUUU_UUUUUUUU_UUUUUUUU_UUUUUUUU_UUUUUUUU_UUUUUUUU", state);
            }
        }
        #endregion 
        #region SHL
        [TestMethod]
        public void Test_MnemonicZ3_Shl_1a()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;

            string line1 = "mov rax, 1";
            string line2 = "shl rax, 1"; // normal behaviour: shift with count 1

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, "00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000010", state);
                TestTools.AreEqual(Flags.CF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.OF, Tv.ZERO, state); //the OF flag is set to 0 if the most-significant bit of the result is the same as the CF flag (that is, the top two bits of the original operand were the same); otherwise, it is set to 1
                TestTools.AreEqual(Flags.AF, Tv.UNDEFINED, state); //For a non-zero count, the AF flag is undefined.
                TestTools.AreEqual(Flags.SF, Tv.ZERO, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.ZF, Tv.ZERO, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.PF, Tv.ZERO, state); // regular behaviour according to the result.
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Shl_1b()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RCX = true;

            string line1 = "mov rax, 1";
            string line2 = "mov rcx, 1";
            string line3 = "shl rax, cl"; // normal behaviour: shift with count 1

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, "00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000010", state);
                TestTools.AreEqual(Flags.CF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.OF, Tv.ZERO, state); //the OF flag is set to 0 if the most-significant bit of the result is the same as the CF flag (that is, the top two bits of the original operand were the same); otherwise, it is set to 1
                TestTools.AreEqual(Flags.AF, Tv.UNDEFINED, state); //For a non-zero count, the AF flag is undefined.
                TestTools.AreEqual(Flags.SF, Tv.ZERO, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.ZF, Tv.ZERO, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.PF, Tv.ZERO, state); // regular behaviour according to the result.
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Shl_2a()
        {   // normal behaviour: shift with count 3: no carry
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;

            string line1 = "mov rax, 1";
            string line2 = "shl rax, 3";

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, "00000000_00000000_00000000_00000000_00000000_00000000_00000000_00001000", state);
                TestTools.AreEqual(Flags.CF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.OF, Tv.UNDEFINED, state); //The OF flag is affected only for 1-bit shifts; otherwise, it is undefined.
                TestTools.AreEqual(Flags.AF, Tv.UNDEFINED, state); //For a non-zero count, the AF flag is undefined.
                TestTools.AreEqual(Flags.SF, Tv.ZERO, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.ZF, Tv.ZERO, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.PF, Tv.ZERO, state); // regular behaviour according to the result.
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Shl_2b()
        {   // normal behaviour: shift with count 3: carry set
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;

            string line1 = "mov rax, 00100000_00000000_00000000_00000000_00000000_00000000_00000000_00000001b";
            string line2 = "shl rax, 3"; // normal behaviour: shift with count 3

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, "00000000_00000000_00000000_00000000_00000000_00000000_00000000_00001000", state);
                TestTools.AreEqual(Flags.CF, Tv.ONE, state);
                TestTools.AreEqual(Flags.OF, Tv.UNDEFINED, state); //The OF flag is affected only for 1-bit shifts; otherwise, it is undefined.
                TestTools.AreEqual(Flags.AF, Tv.UNDEFINED, state); //For a non-zero count, the AF flag is undefined.
                TestTools.AreEqual(Flags.SF, Tv.ZERO, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.ZF, Tv.ZERO, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.PF, Tv.ZERO, state); // regular behaviour according to the result.
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Shl_3()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RCX = true;

            string line1 = "xor rcx, rcx";
            string line2 = "mov rax, 1";
            string line3 = "shl rax, 0"; // special behaviour: shift left zero positions

            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);

                Tv cf = state.GetTv(Flags.CF);
                Tv of = state.GetTv(Flags.OF);
                Tv af = state.GetTv(Flags.AF);
                Tv sf = state.GetTv(Flags.SF);
                Tv zf = state.GetTv(Flags.ZF);
                Tv pf = state.GetTv(Flags.PF);

                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, "00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000001", state);

                //If the count is 0, the flags are not affected.
                Assert.AreEqual(state.GetTv(Flags.CF), cf);
                Assert.AreEqual(state.GetTv(Flags.OF), of);
                Assert.AreEqual(state.GetTv(Flags.AF), af);
                Assert.AreEqual(state.GetTv(Flags.SF), sf);
                Assert.AreEqual(state.GetTv(Flags.ZF), zf);
                Assert.AreEqual(state.GetTv(Flags.PF), pf);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Shl_4()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;

            string line1 = "mov rax, 1";
            string line2 = "shl rax, 65"; // special behaviour: shift left too large; 65 mod 64 = 1

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, "00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000010", state);
                TestTools.AreEqual(Flags.CF, Tv.UNDEFINED, state); //The CF is undefined for SHL and SHR instructions where the count is greater than or equal to the size (in bits) of the destination operand.
                TestTools.AreEqual(Flags.OF, Tv.UNDEFINED, state); //The OF flag is affected only for 1-bit shifts; otherwise, it is undefined.
                TestTools.AreEqual(Flags.AF, Tv.UNDEFINED, state); //For a non-zero count, the AF flag is undefined.
                TestTools.AreEqual(Flags.SF, Tv.ZERO, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.ZF, Tv.ZERO, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.PF, Tv.ZERO, state); // regular behaviour according to the result.
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Shl_5a()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;

            string line1 = "mov eax, 01000000_00000000_00000000_00000010b";
            string line2 = "shl eax, 2"; // normal behaviour: shift left 2

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, "00000000_00000000_00000000_00000000_00000000_00000000_00000000_00001000", state);
                TestTools.AreEqual(Flags.CF, Tv.ONE, state); //The CF is undefined for SHL and SHR instructions where the count is greater than or equal to the size (in bits) of the destination operand.
                TestTools.AreEqual(Flags.OF, Tv.UNDEFINED, state); //The OF flag is affected only for 1-bit shifts; otherwise, it is undefined.
                TestTools.AreEqual(Flags.AF, Tv.UNDEFINED, state); //For a non-zero count, the AF flag is undefined.
                TestTools.AreEqual(Flags.SF, Tv.ZERO, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.ZF, Tv.ZERO, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.PF, Tv.ZERO, state); // regular behaviour according to the result.
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Shl_5b()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;

            string line1 = "mov eax, 00000000_00000000_11000000_00000010b";
            string line2 = "shl ax, 1"; // normal behaviour: shift left 2

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, "00000000_00000000_00000000_00000000_00000000_00000000_10000000_00000100", state);
                TestTools.AreEqual(Flags.CF, Tv.ONE, state); //The CF is undefined for SHL and SHR instructions where the count is greater than or equal to the size (in bits) of the destination operand.
                TestTools.AreEqual(Flags.OF, Tv.ZERO, state); //The OF flag is affected only for 1-bit shifts; otherwise, it is undefined.
                TestTools.AreEqual(Flags.AF, Tv.UNDEFINED, state); //For a non-zero count, the AF flag is undefined.
                TestTools.AreEqual(Flags.SF, Tv.ONE, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.ZF, Tv.ZERO, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.PF, Tv.ZERO, state); // regular behaviour according to the result.
            }
        }
        #endregion
        #region SHRD

        [TestMethod]
        public void Test_MnemonicZ3_Shrd_1a()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            string line1 = "mov rax, 0";
            string line2 = "mov rbx, 0xFFFF_FFFF_FFFF_FFFF";
            string line3 = "shrd rax, rbx, 1"; // normal behaviour: shift with count 1

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, "10000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000", state);
                TestTools.AreEqual(Flags.CF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.OF, Tv.ZERO, state); //the OF flag is set to 0 if the most-significant bit of the result is the same as the CF flag (that is, the top two bits of the original operand were the same); otherwise, it is set to 1
                TestTools.AreEqual(Flags.AF, Tv.UNDEFINED, state); //For a non-zero count, the AF flag is undefined.
                TestTools.AreEqual(Flags.SF, Tv.ONE, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.ZF, Tv.ZERO, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.PF, Tv.ONE, state); // regular behaviour according to the result.
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Shrd_1b()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RCX = true;

            string line1 = "mov rax, 0";
            string line2 = "mov rbx, 0xFFFF_FFFF_FFFF_FFFF";
            string line3 = "mov cl, 1";
            string line4 = "shrd rax, rbx, cl"; // normal behaviour: shift with count 1

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                state = Runner.SimpleStep_Forward(line4, state);
                if (logToDisplay) Console.WriteLine("After \"" + line4 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, "10000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000", state);
                TestTools.AreEqual(Flags.CF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.OF, Tv.ZERO, state); //the OF flag is set to 0 if the most-significant bit of the result is the same as the CF flag (that is, the top two bits of the original operand were the same); otherwise, it is set to 1
                TestTools.AreEqual(Flags.AF, Tv.UNDEFINED, state); //For a non-zero count, the AF flag is undefined.
                TestTools.AreEqual(Flags.SF, Tv.ONE, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.ZF, Tv.ZERO, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.PF, Tv.ONE, state); // regular behaviour according to the result.
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Shrd_2a()
        {   // normal behaviour: shift with count 3: no carry
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            string line1 = "mov rax, 00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000b";
            string line2 = "mov rbx, 0xFFFF_FFFF_FFFF_FFFF";
            string line3 = "shrd rax, rbx, 3"; // normal behaviour: shift with count 3

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, "11100000_00000000_00000000_00000000_00000000_00000000_00000000_00000000", state);
                TestTools.AreEqual(Flags.CF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.OF, Tv.UNDEFINED, state); //The OF flag is affected only for 1-bit shifts; otherwise, it is undefined.
                TestTools.AreEqual(Flags.AF, Tv.UNDEFINED, state); //For a non-zero count, the AF flag is undefined.
                TestTools.AreEqual(Flags.SF, Tv.ONE, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.ZF, Tv.ZERO, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.PF, Tv.ONE, state); // regular behaviour according to the result.
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Shrd_2b()
        {   // normal behaviour: shift with count 3: carry set
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            string line1 = "mov rax, 00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000100b";
            string line2 = "mov rbx, 0xFFFF_FFFF_FFFF_FFFF";
            string line3 = "shrd rax, rbx, 3"; // normal behaviour: shift with count 3

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, "11100000_00000000_00000000_00000000_00000000_00000000_00000000_00000000", state);
                TestTools.AreEqual(Flags.CF, Tv.ONE, state);
                TestTools.AreEqual(Flags.OF, Tv.UNDEFINED, state); //The OF flag is affected only for 1-bit shifts; otherwise, it is undefined.
                TestTools.AreEqual(Flags.AF, Tv.UNDEFINED, state); //For a non-zero count, the AF flag is undefined.
                TestTools.AreEqual(Flags.SF, Tv.ONE, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.ZF, Tv.ZERO, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.PF, Tv.ONE, state); // regular behaviour according to the result.
            }
        }

        #endregion
        #region SHLD
        [TestMethod]
        public void Test_MnemonicZ3_Shld_1a()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            string line1 = "mov rax, 0";
            string line2 = "mov rbx, 0xFFFF_FFFF_FFFF_FFFF";
            string line3 = "shld rax, rbx, 1"; // normal behaviour: shift with count 1

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, "10000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000", state);
                TestTools.AreEqual(Flags.CF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.OF, Tv.ZERO, state); //the OF flag is set to 0 if the most-significant bit of the result is the same as the CF flag (that is, the top two bits of the original operand were the same); otherwise, it is set to 1
                TestTools.AreEqual(Flags.AF, Tv.UNDEFINED, state); //For a non-zero count, the AF flag is undefined.
                TestTools.AreEqual(Flags.SF, Tv.ONE, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.ZF, Tv.ZERO, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.PF, Tv.ONE, state); // regular behaviour according to the result.
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Shld_1b()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RCX = true;

            string line1 = "mov rax, 0";
            string line2 = "mov rbx, 0xFFFF_FFFF_FFFF_FFFF";
            string line3 = "mov cl, 1";
            string line4 = "shld rax, rbx, cl"; // normal behaviour: shift with count 1

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                state = Runner.SimpleStep_Forward(line4, state);
                if (logToDisplay) Console.WriteLine("After \"" + line4 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, "10000000_00000000_00000000_00000000_00000000_00000000_00000000_00000000", state);
                TestTools.AreEqual(Flags.CF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.OF, Tv.ZERO, state); //the OF flag is set to 0 if the most-significant bit of the result is the same as the CF flag (that is, the top two bits of the original operand were the same); otherwise, it is set to 1
                TestTools.AreEqual(Flags.AF, Tv.UNDEFINED, state); //For a non-zero count, the AF flag is undefined.
                TestTools.AreEqual(Flags.SF, Tv.ONE, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.ZF, Tv.ZERO, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.PF, Tv.ONE, state); // regular behaviour according to the result.
            }
        }
        #endregion

        [TestMethod]
        public void Test_MnemonicZ3_Xadd()
        {   // normal behaviour: shift with count 3: carry set
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            string line1 = "mov rax, 00000000_00000000_11111111_11111111_00000000_00000000_11111111_11111111b";
            string line2 = "mov rbx, 00000000_00000001_00000000_00000000_00000000_00000001_00000000_00000000b";
            string line3 = "xadd rax, rbx"; // normal behaviour

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, "00000000_00000001_11111111_11111111_00000000_00000001_11111111_11111111", state);
                TestTools.AreEqual(Rn.RBX, "00000000_00000000_11111111_11111111_00000000_00000000_11111111_11111111", state);

                TestTools.AreEqual(Flags.CF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.OF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.AF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.SF, Tv.ZERO, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.ZF, Tv.ZERO, state); // regular behaviour according to the result.
                TestTools.AreEqual(Flags.PF, Tv.ONE, state); // regular behaviour according to the result.
            }
        }

        #region Imul
        [TestMethod]
        public void Test_MnemonicZ3_Imul_8bits_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            sbyte al = 0b0000_0100;
            sbyte bl = 0b0000_0100;
            short ax = (short)((int)al * (int)bl);

            string line1 = "mov al, " + al;
            string line2 = "mov bl, " + bl;
            string line3 = "imul bl";

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.AX, (ulong)ax, state);
                TestTools.AreEqual(Flags.CF | Flags.OF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF, Tv.UNDEFINED, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Imul_8bits_2()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            sbyte al = 0b0100_0000;
            sbyte bl = 0b0010_0000;
            short ax = (short)((int)al * (int)bl);

            string line1 = "mov al, " + al;
            string line2 = "mov bl, " + bl;
            string line3 = "imul bl";

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.AX, (ulong)ax, state);
                TestTools.AreEqual(Flags.CF | Flags.OF, Tv.ONE, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF, Tv.UNDEFINED, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Imul_8bits_3()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            sbyte al = unchecked((sbyte)0b1000_0000);
            sbyte bl = 0b0010_0000;
            short ax = (short)((int)al * (int)bl);

            string line1 = "mov al, " + al;
            string line2 = "mov bl, " + bl;
            string line3 = "imul bl";

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.AX, (ulong)ax, state);
                TestTools.AreEqual(Flags.CF | Flags.OF, Tv.ONE, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF, Tv.UNDEFINED, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Imul_16bits_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RDX = true;

            short ax = -16;
            short bx = 22;
            int result = (int)((int)ax * (int)bx);
            ulong resultAx = ((ulong)result) & 0xFFFF;
            ulong resultDx = ((ulong)result >> 16) & 0xFFFF;

            {   // forward
                string line1 = "mov ax, " + ax;
                string line2 = "mov bx, " + bx;
                string line3 = "imul bx";

                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.AX, resultAx, state);
                TestTools.AreEqual(Rn.DX, resultDx, state);
                TestTools.AreEqual(Flags.CF | Flags.OF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF, Tv.UNDEFINED, state);
            }
            {   // forward
                string line1 = "mov ax, " + ax;
                string line2 = "mov bx, " + bx;
                string line3 = "imul ax, bx";

                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.AX, resultAx, state);
                TestTools.AreEqual(Flags.CF | Flags.OF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF, Tv.UNDEFINED, state);
            }
            {   // forward
                string line1 = "mov bx, " + bx;
                string line2 = "imul ax, bx, " + ax;

                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.AX, resultAx, state);
                TestTools.AreEqual(Flags.CF | Flags.OF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF, Tv.UNDEFINED, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Imul_16bits_2()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.Set_All_Flags_On();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RDX = true;

            short ax = 0b0100_0000_0000_0000; // large positive
            short bx = -0b0110_0000_0000_0000; // large negative
            int result = (int)((int)ax * (int)bx);
            ulong resultAx = ((ulong)result) & 0xFFFF;
            ulong resultDx = ((ulong)result >> 16) & 0xFFFF;

            {   // forward
                string line1 = "mov ax, " + ax;
                string line2 = "mov bx, " + bx;
                string line3 = "imul bx";

                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.AX, resultAx, state);
                TestTools.AreEqual(Rn.DX, resultDx, state);
                TestTools.AreEqual(Flags.CF | Flags.OF, Tv.ONE, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF, Tv.UNDEFINED, state);
            }
            {   // forward
                string line1 = "mov ax, " + ax;
                string line2 = "mov bx, " + bx;
                string line3 = "imul ax, bx";

                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.AX, resultAx, state);
                TestTools.AreEqual(Flags.CF | Flags.OF, Tv.ONE, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF, Tv.UNDEFINED, state);
            }
            {   // forward
                string line1 = "mov bx, " + bx;
                string line2 = "imul ax, bx, " + ax;

                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.AX, resultAx, state);
                TestTools.AreEqual(Flags.CF | Flags.OF, Tv.ONE, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF, Tv.UNDEFINED, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Imul_32bits_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RDX = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.OF = true;
            tools.StateConfig.AF = true;
            tools.StateConfig.SF = true;
            tools.StateConfig.ZF = true;
            tools.StateConfig.PF = true;

            int eax = 0b0100_0000_0000_0000;
            int ebx = -0b0110_0000_0000_0000;
            long result = (int)((int)eax * (int)ebx);
            ulong resultEax = ((ulong)result) & 0xFFFF_FFFF;
            ulong resultEdx = ((ulong)result >> 32) & 0xFFFF_FFFF;

            {   // forward
                string line1 = "mov eax, " + eax;
                string line2 = "mov ebx, " + ebx;
                string line3 = "imul ebx";

                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.EAX, resultEax, state);
                TestTools.AreEqual(Rn.EDX, resultEdx, state);
                TestTools.AreEqual(Flags.CF | Flags.OF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF, Tv.UNDEFINED, state);
            }
            {   // forward
                string line1 = "mov eax, " + eax;
                string line2 = "mov ebx, " + ebx;
                string line3 = "imul eax, ebx";

                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.EAX, resultEax, state);
                TestTools.AreEqual(Flags.CF | Flags.OF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF, Tv.UNDEFINED, state);
            }
            {   // forward
                string line1 = "mov ebx, " + ebx;
                string line2 = "imul eax, ebx, " + eax;

                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.EAX, resultEax, state);
                TestTools.AreEqual(Flags.CF | Flags.OF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF, Tv.UNDEFINED, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Imul_32bits_2()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RDX = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.OF = true;
            tools.StateConfig.AF = true;
            tools.StateConfig.SF = true;
            tools.StateConfig.ZF = true;
            tools.StateConfig.PF = true;

            int eax = 0b0100_0000_0000_0000_0000_0000_0000_0000; // large positive
            int ebx = -0b0110_0000_0000_0000_0100_0000_0000_0000; // large negative
            long result = (long)eax * (int)ebx;
            ulong resultEax = ((ulong)result) & 0xFFFF_FFFF;
            ulong resultEdx = ((ulong)result >> 32) & 0xFFFF_FFFF;

            {   // forward
                string line1 = "mov eax, " + eax;
                string line2 = "mov ebx, " + ebx;
                string line3 = "imul ebx";

                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.EAX, resultEax, state);
                TestTools.AreEqual(Rn.EDX, resultEdx, state);
                TestTools.AreEqual(Flags.CF | Flags.OF, Tv.ONE, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF, Tv.UNDEFINED, state);
            }
            {   // forward
                string line1 = "mov eax, " + eax;
                string line2 = "mov ebx, " + ebx;
                string line3 = "imul eax, ebx";

                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.EAX, resultEax, state);
                TestTools.AreEqual(Flags.CF | Flags.OF, Tv.ONE, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF, Tv.UNDEFINED, state);
            }
            {   // forward
                string line1 = "mov ebx, " + ebx;
                string line2 = "imul eax, ebx, " + eax;

                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.EAX, resultEax, state);
                TestTools.AreEqual(Flags.CF | Flags.OF, Tv.ONE, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF, Tv.UNDEFINED, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Imul_64bits_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RDX = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.OF = true;
            tools.StateConfig.AF = true;
            tools.StateConfig.SF = true;
            tools.StateConfig.ZF = true;
            tools.StateConfig.PF = true;

            long rax = 0b0100_0000_0000_0000_0000_0000_0000_0000;
            long rbx = -0b0110_0000_0000_0000_0100_0000_0000_0000;

            BigInteger result = BigInteger.Multiply(new BigInteger(rax), new BigInteger(rbx));
            byte[] rawResult = result.ToByteArray();
            ulong resultRax = BitConverter.ToUInt64(rawResult, 0);
            ulong resultRdx = 0xFFFF_FFFF_FFFF_FFFF;// BitConverter.ToUInt64(rawResult, 8);

            {   // forward
                string line1 = "mov rax, " + rax;
                string line2 = "mov rbx, " + rbx;
                string line3 = "imul rbx";

                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, resultRax, state);
                TestTools.AreEqual(Rn.RDX, resultRdx, state);
                TestTools.AreEqual(Flags.CF | Flags.OF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF, Tv.UNDEFINED, state);
            }
            {   // forward
                string line1 = "mov rax, " + rax;
                string line2 = "mov rbx, " + rbx;
                string line3 = "imul rax, rbx";

                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, resultRax, state);
                TestTools.AreEqual(Flags.CF | Flags.OF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF, Tv.UNDEFINED, state);
            }
            {   // forward
                string line1 = "mov rbx, " + rbx;
                string line2 = "imul rax, rbx, " + rax;

                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, resultRax, state);
                TestTools.AreEqual(Flags.CF | Flags.OF, Tv.ZERO, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF, Tv.UNDEFINED, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Imul_64bits_2()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RDX = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.OF = true;
            tools.StateConfig.AF = true;
            tools.StateConfig.SF = true;
            tools.StateConfig.ZF = true;
            tools.StateConfig.PF = true;

            long rax = 0b0100_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000;
            long rbx = -0b0110_0000_0000_0000_0100_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000; // large negative

            BigInteger result = BigInteger.Multiply(new BigInteger(rax), new BigInteger(rbx));
            byte[] rawResult = result.ToByteArray();
            //Console.WriteLine("rawResult.length=" + rawResult.Length);
            ulong resultRax = BitConverter.ToUInt64(rawResult, 0);
            ulong resultRdx = BitConverter.ToUInt64(rawResult, 8);

            {   // forward
                string line1 = "mov rax, " + rax;
                string line2 = "mov rbx, " + rbx;
                string line3 = "imul rbx";

                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, resultRax, state);
                TestTools.AreEqual(Rn.RDX, resultRdx, state);
                TestTools.AreEqual(Flags.CF | Flags.OF, Tv.ONE, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF, Tv.UNDEFINED, state);
            }
            {   // forward
                string line1 = "mov rax, " + rax;
                string line2 = "mov rbx, " + rbx;
                string line3 = "imul rax, rbx";

                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, resultRax, state);
                TestTools.AreEqual(Flags.CF | Flags.OF, Tv.ONE, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF, Tv.UNDEFINED, state);
            }
        }

        #endregion Imul

        #region Div
        [TestMethod]
        public void Test_MnemonicZ3_Div_8bits_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RDX = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.OF = true;
            tools.StateConfig.AF = true;
            tools.StateConfig.SF = true;
            tools.StateConfig.ZF = true;
            tools.StateConfig.PF = true;

            UInt16 ax = 0b0000_0000_0100_0000;
            byte bl = 0b0000_0010;
            byte quotient = (byte)(ax / bl);
            byte remainder = (byte)(ax % bl);

            string line1 = "mov ax, " + ax;
            string line2 = "mov bl, " + bl;
            string line3 = "div bl";

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.AL, quotient, state);
                TestTools.AreEqual(Rn.AH, remainder, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF | Flags.CF | Flags.OF, Tv.UNDEFINED, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Div_8bits_2()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.OF = true;
            tools.StateConfig.AF = true;
            tools.StateConfig.SF = true;
            tools.StateConfig.ZF = true;
            tools.StateConfig.PF = true;

            UInt16 ax = 0b0000_0011_0100_0000;
            byte bl = 0b0000_0100;
            byte quotient = (byte)(ax / bl);
            byte remainder = (byte)(ax % bl);

            string line1 = "mov ax, " + ax;
            string line2 = "mov bl, " + bl;
            string line3 = "div bl";

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.AL, quotient, state);
                TestTools.AreEqual(Rn.AH, remainder, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF | Flags.CF | Flags.OF, Tv.UNDEFINED, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Div_16bits_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RDX = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.OF = true;
            tools.StateConfig.AF = true;
            tools.StateConfig.SF = true;
            tools.StateConfig.ZF = true;
            tools.StateConfig.PF = true;

            UInt16 dx = 0b0000_0000_0000_0100;
            UInt16 ax = 0b0000_0011_0100_0000;
            UInt16 bx = 0b0000_0000_0100_0000;

            UInt32 value = (((UInt32)dx) << 16) | (UInt32)ax;
            UInt16 quotient = (UInt16)(value / (UInt32)bx);
            UInt16 remainder = (UInt16)(value % (UInt32)bx);

            string line1 = "mov ax, " + ax;
            string line2 = "mov dx, " + dx;
            string line3 = "mov bx, " + bx;
            string line4 = "div bx";

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line4, state);
                if (logToDisplay) Console.WriteLine("After \"" + line4 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.AX, (ulong)quotient, state);
                TestTools.AreEqual(Rn.DX, (ulong)remainder, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF | Flags.CF | Flags.OF, Tv.UNDEFINED, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Div_32bits_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RDX = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.OF = true;
            tools.StateConfig.AF = true;
            tools.StateConfig.SF = true;
            tools.StateConfig.ZF = true;
            tools.StateConfig.PF = true;

            UInt32 edx = 0b0000_0000_0000_0100_0000_0000_0000_0100;
            UInt32 eax = 0b0000_0000_0000_0000_0000_0011_0100_0000;
            UInt32 ebx = 0b0000_0000_0000_1000_0000_0000_0100_0000;

            UInt64 value = (((UInt64)edx) << 32) | (UInt64)eax;
            UInt32 quotient = (UInt32)(value / (UInt64)ebx);
            UInt32 remainder = (UInt32)(value % (UInt64)ebx);

            string line1 = "mov eax, " + eax;
            string line2 = "mov edx, " + edx;
            string line3 = "mov ebx, " + ebx;
            string line4 = "div ebx";

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line4, state);
                if (logToDisplay) Console.WriteLine("After \"" + line4 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.EAX, (ulong)quotient, state);
                TestTools.AreEqual(Rn.EDX, (ulong)remainder, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF | Flags.CF | Flags.OF, Tv.UNDEFINED, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Div_64bits_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RDX = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.OF = true;
            tools.StateConfig.AF = true;
            tools.StateConfig.SF = true;
            tools.StateConfig.ZF = true;
            tools.StateConfig.PF = true;

            UInt64 rdx = 0x0000_0010_FFFF_FFFF;
            UInt64 rax = 0x0000_0000_1000_F000;
            UInt64 rbx = 0x0000_0100_FFFF_FFFF;

            BigInteger value = ((new BigInteger(rdx)) << 64) + new BigInteger(rax);
            BigInteger quotient = BigInteger.DivRem(value, new BigInteger(rbx), out BigInteger remainder);

            //quotient = 0x10ef_10ef_1000_ee22 = 1220212642992418338
            //rem = 0x0000_00cd_2001_de22 = 881005288994

            ulong quotient_ulong = BitConverter.ToUInt64(quotient.ToByteArray(), 0);
            ulong remainder_ulong = (ulong)remainder;

            string line1 = "mov rax, " + rax;
            string line2 = "mov rdx, " + rdx;
            string line3 = "mov rbx, " + rbx;
            string line4 = "div rbx";

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);
                state = Runner.SimpleStep_Forward(line4, state);
                if (logToDisplay) Console.WriteLine("After \"" + line4 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RAX, quotient_ulong, state);
                TestTools.AreEqual(Rn.RDX, remainder_ulong, state);
                TestTools.AreEqual(Flags.AF | Flags.SF | Flags.ZF | Flags.PF | Flags.CF | Flags.OF, Tv.UNDEFINED, state);
            }
        }

        #endregion Div

        #region Push/Pop
        [TestMethod]
        public void Test_MnemonicZ3_Push_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RSP = true;
            tools.StateConfig.mem = true;

            string line1 = "mov rsp, 0x3FFF";
            string line2 = "mov rax, 00000000_00000001_00000000_00000000_00000000_00000001_00000000_00000000b";
            string line3 = "push rax";

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RSP, 0x3FFF - 8, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Pop_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RSP = true;
            tools.StateConfig.mem = true;

            string line1 = "mov rsp, 0x3FFF";
            string line2 = "pop rax";

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RSP, 0x3FFF + 8, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_PushPop_64bit_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RSP = true;
            tools.StateConfig.mem = true;

            string line1 = "mov rsp, 0x3FFF";
            string line2 = "mov rax, 0";
            string line3 = "push rax";
            string line4 = "pop rbx";

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                state = Runner.SimpleStep_Forward(line4, state);
                if (logToDisplay) Console.WriteLine("After \"" + line4 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RSP, 0x3FFF, state);
                TestTools.AreEqual(Rn.RAX, Rn.RBX, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_PushPop_64bit_2()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RSP = true;
            tools.StateConfig.mem = true;

            string line1 = "mov rsp, 0x3FFF";
            string line2 = "push rax";
            string line3 = "pop rbx";

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                state = Runner.SimpleStep_Forward(line3, state);
                if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RSP, 0x3FFF, state);
                TestTools.AreEqual(Rn.RAX, Rn.RBX, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_PushPop_64bit_3()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RSP = true;
            tools.StateConfig.mem = true;

            string line1 = "push rax";
            string line2 = "pop rbx";

            {   // forward
                State state = CreateState(tools);
                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, Rn.RBX, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_PushPop_64bit_4()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;
            tools.StateConfig.RSP = true;
            tools.StateConfig.mem = true;

            string line1 = "push rax";
            string line2 = "pop rbx";

            {   // forward
                State state = CreateState(tools);
                Context ctx = state.Ctx;

                StateUpdate updateState = new StateUpdate("!PREVKEY", "!NEXTKEY", state.Tools);
                updateState.Set(Rn.RAX, "0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_00U0");
                state.Update_Forward(updateState);

                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
                TestTools.AreEqual(Rn.RAX, Rn.RBX, state);
            }
        }

        #endregion Push/Pop

        #region In
        [TestMethod]
        public void Test_MnemonicZ3_In_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;

            string line1 = "mov eax, 0";
            string line2 = "in eax, 8";

            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.EAX, "????_????_????_????_????_????_????_????", state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_In_2()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;

            string line1 = "in eax, 8";

            {   // forward
                State state = CreateState(tools);
                Context ctx = state.Ctx;

                StateUpdate updateState = new StateUpdate("!PREVKEY", "!NEXTKEY", state.Tools);
                updateState.Set(Rn.EAX, "????_????_????_????_????_????_????_UU??");
                state.Update_Forward(updateState);

                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.EAX, "????_????_????_????_????_????_????_????", state);
            }
        }
        #endregion in

        #region Popcnt 
        [TestMethod]
        public void Test_MnemonicZ3_Popcnt_64bits_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            string line1 = "mov rax, 5";
            string line2 = "popcnt rbx, rax";

            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RBX, 2, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Popcnt_64bits_2()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            string line1 = "mov rax, 0xFFFF_FFFF_FFFF_FFFF";
            string line2 = "popcnt rbx, rax";

            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.RBX, 64, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Popcnt_32bits_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            string line1 = "mov eax, 2";
            string line2 = "popcnt ebx, eax";

            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.EBX, 1, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Popcnt_32bits_3()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            string line1 = "mov eax, 0xFFFF_FFFF";
            string line2 = "popcnt ebx, eax";

            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.EBX, 32, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Popcnt_16bits_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            string line1 = "mov ax, 2";
            string line2 = "popcnt bx, ax";

            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.BX, 1, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Popcnt_16bits_2()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            string line1 = "mov ax, 0xFFFF";
            string line2 = "popcnt bx, ax";

            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                state = Runner.SimpleStep_Forward(line2, state);
                if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.BX, 16, state);
            }
        }

        [TestMethod]
        public void Test_MnemonicZ3_Popcnt_16bits_3()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.RBX = true;

            string line1 = "popcnt bx, ax";

            {   // forward
                State state = CreateState(tools);

                state = Runner.SimpleStep_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);

                TestTools.AreEqual(Rn.BX, "0000_0000_000?_????", state);
            }
        }
        #endregion Popcnt

        #region Jcc 
        [TestMethod]
        public void Test_MnemonicZ3_Jcc_1()
        {
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.RAX = true;
            tools.StateConfig.CF = true;

            string line1 = "jc label";
 
            {   // forward
                State state = CreateState(tools);

                (State state1a, State state1b) = Runner.Step_Forward(line1, state);
                if (logToDisplay) Console.WriteLine("State1A: After \"" + line1 + "\", we know:\n" + state1a);
                if (logToDisplay) Console.WriteLine("State1B: After \"" + line1 + "\", we know:\n" + state1b);

//                TestTools.AreEqual(Rn.RBX, 2, state);
            }
        }


        #endregion

        #region Decimal Arithmetic Instructions

        #region DAA
        [TestMethod]
        public void Test_MnemonicZ3_Daa_1()
        {
            // no overflow

            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.AF = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.RAX = true;

            int byteA_1 = 4;
            int byteA_2 = 3;
            int byteB_1 = 2;
            int byteB_2 = 1;
            int result = (((byteA_2+byteB_2) << 4) | ((byteA_1 + byteB_1) << 0));

            string line1 = "mov eax, " + ((byteA_2 << 4) | (byteA_1 << 0));
            string line2 = "add eax, " + ((byteB_2 << 4) | (byteB_1 << 0));
            string line3 = "daa";

            State state = CreateState(tools);
            state = Runner.SimpleStep_Forward(line1, state);
            state = Runner.SimpleStep_Forward(line2, state);
            state = Runner.SimpleStep_Forward(line3, state);
            if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

            TestTools.AreEqual(Rn.AL, result, state);
        }
        [TestMethod]
        public void Test_MnemonicZ3_Daa_2()
        {
            // with overflow
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.AF = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.RAX = true;

            int byteA_1 = 8;
            int byteA_2 = 3;
            int byteB_1 = 3;
            int byteB_2 = 1;
            int result = (((byteA_2 + byteB_2 + 1) << 4) | (1 << 0));

            string line1 = "mov eax, " + ((byteA_2 << 4) | (byteA_1 << 0));
            string line2 = "add eax, " + ((byteB_2 << 4) | (byteB_1 << 0));
            string line3 = "daa";

            State state = CreateState(tools);
            state = Runner.SimpleStep_Forward(line1, state);
            state = Runner.SimpleStep_Forward(line2, state);
            state = Runner.SimpleStep_Forward(line3, state);
            if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

            TestTools.AreEqual(Rn.AL, result, state);
        }


        #endregion
        #region DAS
        [TestMethod]
        public void Test_MnemonicZ3_Das_1()
        {
            // no overflow

            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.AF = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.RAX = true;

            //34 - 12 = 22
            int byteA_1 = 4;
            int byteA_2 = 3;
            int byteB_1 = 2;
            int byteB_2 = 1;
            int result = (((byteA_2 - byteB_2) << 4) | ((byteA_1 - byteB_1) << 0));

            string line1 = "mov eax, " + ((byteA_2 << 4) | (byteA_1 << 0));
            string line2 = "sub eax, " + ((byteB_2 << 4) | (byteB_1 << 0));
            string line3 = "das";

            State state = CreateState(tools);
            state = Runner.SimpleStep_Forward(line1, state);
            state = Runner.SimpleStep_Forward(line2, state);
            state = Runner.SimpleStep_Forward(line3, state);
            if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

            TestTools.AreEqual(Rn.AL, result, state);
        }
        [TestMethod]
        public void Test_MnemonicZ3_Das_2()
        {
            // with overflow
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.AF = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.RAX = true;

            //32 - 14 = 18
            int byteA_1 = 2;
            int byteA_2 = 3;
            int byteB_1 = 4;
            int byteB_2 = 1;
            int result = (1 << 4) | (8 << 0);

            string line1 = "mov eax, " + ((byteA_2 << 4) | (byteA_1 << 0));
            string line2 = "sub eax, " + ((byteB_2 << 4) | (byteB_1 << 0));
            string line3 = "das";

            State state = CreateState(tools);
            state = Runner.SimpleStep_Forward(line1, state);
            state = Runner.SimpleStep_Forward(line2, state);
            state = Runner.SimpleStep_Forward(line3, state);
            if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

            TestTools.AreEqual(Rn.AL, result, state);
        }


        #endregion
        #region AAA
        [TestMethod]
        public void Test_MnemonicZ3_Aaa_1()
        {
            // no overflow

            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.AF = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.RAX = true;

            //4 + 2 = 6
            int byteA_1 = 4;
            int byteB_1 = 2;
            int result = byteA_1 + byteB_1;

            string line1 = "mov ax, " + byteA_1;
            string line2 = "add al, " + byteB_1;
            string line3 = "aaa";

            State state = CreateState(tools);
            state = Runner.SimpleStep_Forward(line1, state);
            state = Runner.SimpleStep_Forward(line2, state);
            state = Runner.SimpleStep_Forward(line3, state);
            if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

            TestTools.AreEqual(Rn.AL, result, state);
            TestTools.AreEqual(Flags.AF, false, state);
            TestTools.AreEqual(Flags.CF, false, state);
        }
        [TestMethod]
        public void Test_MnemonicZ3_Aaa_2()
        {
            // with overflow
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.AF = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.RAX = true;

            //4 + 8 = 12
            int byteA_1 = 4;
            int byteB_1 = 8;
            int al_result = 2;
            int ah_result = 1;

            string line1 = "mov ax, " + byteA_1;
            string line2 = "add al, " + byteB_1;
            string line3 = "aaa";

            State state = CreateState(tools);
            state = Runner.SimpleStep_Forward(line1, state);
            state = Runner.SimpleStep_Forward(line2, state);
            if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
            state = Runner.SimpleStep_Forward(line3, state);
            if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

            TestTools.AreEqual(Rn.AL, al_result, state);
            TestTools.AreEqual(Rn.AH, ah_result, state);
            TestTools.AreEqual(Flags.AF, true, state);
            TestTools.AreEqual(Flags.CF, true, state);
        }


        #endregion
        #region AAS
        [TestMethod]
        public void Test_MnemonicZ3_Aas_1()
        {
            // no overflow

            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.AF = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.RAX = true;

            //4 - 2 = 2
            int byteA_1 = 4;
            int byteB_1 = 2;
            int result = byteA_1 - byteB_1;

            string line1 = "mov ax, " + byteA_1;
            string line2 = "sub al, " + byteB_1;
            string line3 = "aas";

            State state = CreateState(tools);
            state = Runner.SimpleStep_Forward(line1, state);
            state = Runner.SimpleStep_Forward(line2, state);
            state = Runner.SimpleStep_Forward(line3, state);
            if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

            TestTools.AreEqual(Rn.AL, result, state);
            TestTools.AreEqual(Flags.AF, false, state);
            TestTools.AreEqual(Flags.CF, false, state);

        }
        [TestMethod]
        public void Test_MnemonicZ3_Aas_2()
        {
            // with overflow
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.AF = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.RAX = true;

            //24 - 8 = 16
            int byteA_1 = 4;
            int byteA_2 = 2;
            int byteB_1 = 8;
            int al_result = 6;
            int ah_result = 1;

            string line0 = "mov ah, " + byteA_2;
            string line1 = "mov al, " + byteA_1;
            string line2 = "sub al, " + byteB_1;
            string line3 = "aas";

            State state = CreateState(tools);
            state = Runner.SimpleStep_Forward(line0, state);
            state = Runner.SimpleStep_Forward(line1, state);
            state = Runner.SimpleStep_Forward(line2, state);
            if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
            state = Runner.SimpleStep_Forward(line3, state);
            if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

            TestTools.AreEqual(Rn.AL, al_result, state);
            TestTools.AreEqual(Rn.AH, ah_result, state);
            TestTools.AreEqual(Flags.AF, true, state);
            TestTools.AreEqual(Flags.CF, true, state);
        }
        #endregion
        #region AAM
        [TestMethod]
        public void Test_MnemonicZ3_Aam_Base10_1()
        {
            // no overflow
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.AF = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.RAX = true;
            tools.StateConfig.RDX = true;

            int imm8 = 10;

            //4 * 2 = 8
            int byteA_1 = 4;
            int byteD_1 = 2;
            int result = byteA_1 * byteD_1;
            int al_result = result % imm8;
            int ah_result = result / imm8;

            string line0 = "mov al, " + byteA_1;
            string line1 = "mov dl, " + byteD_1;
            string line2 = "mul dl";
            string line3 = "aam";

            State state = CreateState(tools);
            state = Runner.SimpleStep_Forward(line0, state);
            state = Runner.SimpleStep_Forward(line1, state);
            state = Runner.SimpleStep_Forward(line2, state);
            if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
            state = Runner.SimpleStep_Forward(line3, state);
            if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

            TestTools.AreEqual(Rn.AL, al_result, state);
            TestTools.AreEqual(Rn.AH, ah_result, state);
            TestTools.AreEqual(Flags.AF, Tv.UNDEFINED, state);
            TestTools.AreEqual(Flags.CF, Tv.UNDEFINED, state);

        }
        [TestMethod]
        public void Test_MnemonicZ3_Aam_Base10_2()
        {
            // with overflow
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.AF = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.RAX = true;
            tools.StateConfig.RDX = true;

            int imm8 = 10;

            //8 * 2 = 16
            int byteA_1 = 8;
            int byteD_1 = 2;
            int result = byteA_1 * byteD_1;
            int al_result = result % imm8;
            int ah_result = result / imm8;

            string line0 = "mov al, " + byteA_1;
            string line1 = "mov dl, " + byteD_1;
            string line2 = "mul dl";
            string line3 = "aam";

            State state = CreateState(tools);
            state = Runner.SimpleStep_Forward(line0, state);
            state = Runner.SimpleStep_Forward(line1, state);
            state = Runner.SimpleStep_Forward(line2, state);
            if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
            state = Runner.SimpleStep_Forward(line3, state);
            if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

            TestTools.AreEqual(Rn.AL, al_result, state);
            TestTools.AreEqual(Rn.AH, ah_result, state);
            TestTools.AreEqual(Flags.AF, Tv.UNDEFINED, state);
            TestTools.AreEqual(Flags.CF, Tv.UNDEFINED, state);
        }
        [TestMethod]
        public void Test_MnemonicZ3_Aam_Base11_1()
        {
            // no overflow
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.AF = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.RAX = true;
            tools.StateConfig.RDX = true;

            int imm8 = 11;

            //4 * 2 = 8
            int byteA_1 = 4;
            int byteD_1 = 2;
            int result = byteA_1 * byteD_1;
            int al_result = result % imm8;
            int ah_result = result / imm8;

            string line0 = "mov al, " + byteA_1;
            string line1 = "mov dl, " + byteD_1;
            string line2 = "mul dl";
            string line3 = "aam " + imm8;

            State state = CreateState(tools);
            state = Runner.SimpleStep_Forward(line0, state);
            state = Runner.SimpleStep_Forward(line1, state);
            state = Runner.SimpleStep_Forward(line2, state);
            if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
            state = Runner.SimpleStep_Forward(line3, state);
            if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

            TestTools.AreEqual(Rn.AL, al_result, state);
            TestTools.AreEqual(Rn.AH, ah_result, state);
            TestTools.AreEqual(Flags.AF, Tv.UNDEFINED, state);
            TestTools.AreEqual(Flags.CF, Tv.UNDEFINED, state);

        }
        [TestMethod]
        public void Test_MnemonicZ3_Aam_Base11_2()
        {
            // with overflow
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.AF = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.RAX = true;
            tools.StateConfig.RDX = true;

            int imm8 = 11;

            //8 * 2 = 16
            int byteA_1 = 8;
            int byteD_1 = 2;
            int result = byteA_1 * byteD_1;
            int al_result = result % imm8;
            int ah_result = result / imm8;

            string line0 = "mov al, " + byteA_1;
            string line1 = "mov dl, " + byteD_1;
            string line2 = "mul dl";
            string line3 = "aam " + imm8;

            State state = CreateState(tools);
            state = Runner.SimpleStep_Forward(line0, state);
            state = Runner.SimpleStep_Forward(line1, state);
            state = Runner.SimpleStep_Forward(line2, state);
            if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
            state = Runner.SimpleStep_Forward(line3, state);
            if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

            TestTools.AreEqual(Rn.AL, al_result, state);
            TestTools.AreEqual(Rn.AH, ah_result, state);
            TestTools.AreEqual(Flags.AF, Tv.UNDEFINED, state);
            TestTools.AreEqual(Flags.CF, Tv.UNDEFINED, state);
        }
        #endregion
        #region AAD
        [TestMethod]
        public void Test_MnemonicZ3_Aad_Base10_1()
        {
            // no overflow
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.AF = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.RAX = true;
            tools.StateConfig.RDX = true;

            int imm8 = 10;

            //8 / 2 = 4
            int byteA_1 = 8;
            int byteD_1 = 2;
            int result = byteA_1 / byteD_1;
            int al_result = result % imm8;
            
            string line0 = "mov ax, " + byteA_1;
            string line1 = "mov dl, " + byteD_1;
            string line2 = "aad"; // adjust AX BEFORE Division
            string line3 = "div dl";

            State state = CreateState(tools);
            state = Runner.SimpleStep_Forward(line0, state);
            state = Runner.SimpleStep_Forward(line1, state);
            state = Runner.SimpleStep_Forward(line2, state);
            if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
            state = Runner.SimpleStep_Forward(line3, state);
            if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

            TestTools.AreEqual(Rn.AL, al_result, state);
            TestTools.AreEqual(Rn.AH, 0, state);
            TestTools.AreEqual(Flags.AF, Tv.UNDEFINED, state);
            TestTools.AreEqual(Flags.CF, Tv.UNDEFINED, state);

        }
        [TestMethod]
        public void Test_MnemonicZ3_Aad_Base10_2()
        {
            // with overflow
            Tools tools = CreateTools();
            tools.StateConfig.Set_All_Off();
            tools.StateConfig.AF = true;
            tools.StateConfig.CF = true;
            tools.StateConfig.RAX = true;
            tools.StateConfig.RDX = true;

            int imm8 = 10;

            //32 / 2 = 16
            int byteA_2 = 3;
            int byteA_1 = 2;
            int decimalA = ((byteA_2 * 10) + byteA_1);
            int byteD_1 = 2;

            int result = decimalA / byteD_1;
            int al_result = result % imm8;

            string line0 = "mov ax, " + ((byteA_2 << 8) | (byteA_1 << 0));
            string line1 = "mov dl, " + byteD_1;
            string line2 = "aad"; // adjust AX BEFORE Division
            string line3 = "div dl";

            State state = CreateState(tools);
            state = Runner.SimpleStep_Forward(line0, state);
            state = Runner.SimpleStep_Forward(line1, state);
            if (logToDisplay) Console.WriteLine("After \"" + line1 + "\", we know:\n" + state);
            state = Runner.SimpleStep_Forward(line2, state);
            if (logToDisplay) Console.WriteLine("After \"" + line2 + "\", we know:\n" + state);
            TestTools.AreEqual(Rn.AX, decimalA, state);

            state = Runner.SimpleStep_Forward(line3, state);
            if (logToDisplay) Console.WriteLine("After \"" + line3 + "\", we know:\n" + state);

            TestTools.AreEqual(Rn.AL, result, state);
            TestTools.AreEqual(Rn.AH, 0, state);
            TestTools.AreEqual(Flags.AF, Tv.UNDEFINED, state);
            TestTools.AreEqual(Flags.CF, Tv.UNDEFINED, state);
        }
        #endregion
        #endregion
    }
}
