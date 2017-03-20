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

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using dnSpy.Contracts.App;
using dnSpy.Contracts.Debugger;
using dnSpy.Debugger.Dialogs.AttachToProcess;
using dnSpy.Debugger.Properties;

namespace dnSpy.Debugger.DbgUI {
	[Export(typeof(Debugger))]
	[ExportDbgManagerStartListener]
	sealed class DebuggerImpl : Debugger, IDbgManagerStartListener {
		readonly IMessageBoxService messageBoxService;
		readonly IAppWindow appWindow;
		readonly Lazy<DbgManager> dbgManager;
		readonly Lazy<StartDebuggingOptionsProvider> startDebuggingOptionsProvider;
		readonly Lazy<ShowAttachToProcessDialog> showAttachToProcessDialog;

		public override bool IsDebugging => dbgManager.Value.IsDebugging;

		[ImportingConstructor]
		DebuggerImpl(IMessageBoxService messageBoxService, IAppWindow appWindow, Lazy<DbgManager> dbgManager, Lazy<StartDebuggingOptionsProvider> startDebuggingOptionsProvider, Lazy<ShowAttachToProcessDialog> showAttachToProcessDialog) {
			this.messageBoxService = messageBoxService;
			this.appWindow = appWindow;
			this.dbgManager = dbgManager;
			this.startDebuggingOptionsProvider = startDebuggingOptionsProvider;
			this.showAttachToProcessDialog = showAttachToProcessDialog;
		}

		public override bool CanStartWithoutDebugging => startDebuggingOptionsProvider.Value.GetCurrentExecutableFilename() != null;
		public override void StartWithoutDebugging() {
			var filename = startDebuggingOptionsProvider.Value.GetCurrentExecutableFilename();
			if (!File.Exists(filename))
				return;
			try {
				Process.Start(filename);
			}
			catch (Exception ex) {
				messageBoxService.Show(string.Format(dnSpy_Debugger_Resources.Error_StartWithoutDebuggingCouldNotStart, filename, ex.Message));
			}
		}

		public override bool CanDebugProgram => true;
		public override void DebugProgram() {
			var options = startDebuggingOptionsProvider.Value.GetStartDebuggingOptions();
			if (options == null)
				return;

			var errMsg = dbgManager.Value.Start(options);
			if (errMsg != null)
				messageBoxService.Show(errMsg);
		}

		public override bool CanAttachProgram => true;
		public override void AttachProgram() => showAttachToProcessDialog.Value.Attach();

		bool CanExecuteCurrentProcessPauseCommand => dbgManager.Value.IsDebugging && dbgManager.Value.CurrentProcess?.State == DbgProcessState.Paused;
		bool CanExecutePauseCommand => dbgManager.Value.IsDebugging && dbgManager.Value.IsRunning != true;
		bool CanExecuteRunningCommand => dbgManager.Value.IsDebugging && dbgManager.Value.IsRunning != false;
		bool CanExecutePauseOrRunningCommand => dbgManager.Value.IsDebugging;

		public override bool CanContinue => CanExecutePauseCommand;
		public override void Continue() => dbgManager.Value.RunAll();
		public override bool CanBreakAll => CanExecuteRunningCommand;
		public override void BreakAll() => dbgManager.Value.BreakAll();
		public override bool CanStopDebugging => CanExecutePauseOrRunningCommand;
		public override void StopDebugging() => dbgManager.Value.StopDebuggingAll();

		public override bool CanDetachAll => CanExecutePauseOrRunningCommand;
		public override void DetachAll() {
			if (!dbgManager.Value.CanDetachWithoutTerminating) {
				//TODO: Show a msg box
			}
			dbgManager.Value.DetachAll();
		}

		public override bool CanTerminateAll => CanExecutePauseOrRunningCommand;
		public override void TerminateAll() => dbgManager.Value.TerminateAll();
		public override bool CanRestart => CanExecutePauseOrRunningCommand && dbgManager.Value.CanRestart;
		public override void Restart() => dbgManager.Value.Restart();

		public override bool CanShowNextStatement => CanExecutePauseCommand;
		public override void ShowNextStatement() {
			//TODO:
		}

		public override bool CanStepInto => CanExecutePauseCommand;
		public override void StepInto() {
			//TODO:
		}

		public override bool CanStepOver => CanExecutePauseCommand;
		public override void StepOver() {
			//TODO:
		}

		public override bool CanStepOut => CanExecutePauseCommand;
		public override void StepOut() {
			//TODO:
		}

		public override bool CanStepIntoCurrentProcess => CanExecuteCurrentProcessPauseCommand;
		public override void StepIntoCurrentProcess() {
			//TODO:
		}

		public override bool CanStepOverCurrentProcess => CanExecuteCurrentProcessPauseCommand;
		public override void StepOverCurrentProcess() {
			//TODO:
		}

		public override bool CanStepOutCurrentProcess => CanExecuteCurrentProcessPauseCommand;
		public override void StepOutCurrentProcess() {
			//TODO:
		}

		void IDbgManagerStartListener.OnStart(DbgManager dbgManager) {
			dbgManager.IsDebuggingChanged += DbgManager_IsDebuggingChanged;
			dbgManager.IsRunningChanged += DbgManager_IsRunningChanged;
		}

		void UI(Action action) {
			var dispatcher = appWindow.MainWindow.Dispatcher;
			if (!dispatcher.HasShutdownStarted && !dispatcher.HasShutdownFinished)
				dispatcher.BeginInvoke(DispatcherPriority.Send, action);
		}

		void DbgManager_IsDebuggingChanged(object sender, EventArgs e) {
			var dbgManager = (DbgManager)sender;
			UI(() => {
				var newIsDebugging = dbgManager.IsDebugging;
				if (newIsDebugging == oldIsDebugging)
					return;
				oldIsDebugging = newIsDebugging;
				Application.Current.Resources["IsDebuggingKey"] = newIsDebugging;
				if (newIsDebugging) {
					appWindow.StatusBar.Open();
					SetRunningStatusMessage();
					appWindow.AddTitleInfo(dnSpy_Debugger_Resources.AppTitle_Debugging);
				}
				else {
					appWindow.StatusBar.Close();
					appWindow.RemoveTitleInfo(dnSpy_Debugger_Resources.AppTitle_Debugging);
				}
				appWindow.RefreshToolBar();
			});
		}
		bool oldIsDebugging;

		void SetRunningStatusMessage() => appWindow.StatusBar.Show(dnSpy_Debugger_Resources.StatusBar_Running);

		void DbgManager_IsRunningChanged(object sender, EventArgs e) {
			var dbgManager = (DbgManager)sender;
			UI(() => {
				var newIsRunning = dbgManager.IsRunning;
				if (newIsRunning == oldIsRunning)
					return;
				oldIsRunning = newIsRunning;
				CommandManager.InvalidateRequerySuggested();
			});
		}
		bool? oldIsRunning;
	}
}
