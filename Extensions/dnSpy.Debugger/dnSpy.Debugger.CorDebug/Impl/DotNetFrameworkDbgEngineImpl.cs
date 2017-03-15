﻿/*
    Copyright (C) 2014-2017 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Diagnostics;
using dndbg.Engine;
using dnSpy.Contracts.Debugger;
using dnSpy.Contracts.Debugger.DotNet.CorDebug;
using dnSpy.Contracts.Debugger.Engine;
using dnSpy.Debugger.CorDebug.DAC;

namespace dnSpy.Debugger.CorDebug.Impl {
	sealed class DotNetFrameworkDbgEngineImpl : DbgEngineImpl {
		protected override CorDebugRuntimeKind CorDebugRuntimeKind => CorDebugRuntimeKind.DotNetFramework;
		public override string Debugging => "CLR";

		public override string RuntimeName {
			get {
				Debug.Assert(runtimeName != null);
				return runtimeName;
			}
		}
		string runtimeName;

		public DotNetFrameworkDbgEngineImpl(ClrDacProvider clrDacProvider, DbgManager dbgManager, DbgStartKind startKind)
			: base(clrDacProvider, dbgManager, startKind) {
		}

		protected override CLRTypeDebugInfo CreateDebugInfo(CorDebugStartDebuggingOptions options) =>
			new DesktopCLRTypeDebugInfo();

		protected override void OnDebugProcess(DnDebugger dnDebugger) =>
			runtimeName = "CLR " + dnDebugger.DebuggeeVersion;
	}
}
