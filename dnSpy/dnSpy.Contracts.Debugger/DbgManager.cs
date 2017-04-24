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

namespace dnSpy.Contracts.Debugger {
	/// <summary>
	/// Manages all debug engines. All events are raised in the dispatcher thread.
	/// If you need to hook events before debugging starts, you should export an <see cref="IDbgManagerStartListener"/>.
	/// It gets called when <see cref="Start(StartDebuggingOptions)"/> gets called for the first time.
	/// </summary>
	public abstract class DbgManager {
		/// <summary>
		/// Gets the dispatcher. All debugger events are raised in this thread. <see cref="DbgObject.Close(DispatcherThread)"/>
		/// is also called in this thread including disposing of data added by eg. <see cref="DbgObject.GetOrCreateData{T}()"/>.
		/// </summary>
		public abstract DispatcherThread DispatcherThread { get; }

		/// <summary>
		/// Raised on the debugger thread when there's a new message, eg. a process was created, a thread has exited, etc.
		/// The listeners can pause the debugged program by setting <see cref="DbgMessageEventArgs.Pause"/> to true.
		/// </summary>
		public abstract event EventHandler<DbgMessageEventArgs> Message;

		/// <summary>
		/// Gets the debugging context that gets disposed when debugging stops. This is null if <see cref="IsDebugging"/> is false
		/// </summary>
		public abstract DbgDebuggingContext DebuggingContext { get; }

		/// <summary>
		/// Starts debugging. Returns an error string if it failed to create a debug engine, or null on success.
		/// See <see cref="IDbgManagerStartListener"/> on how to get called the first time this method gets called.
		/// </summary>
		/// <param name="options">Options needed to start the program or attach to it</param>
		public abstract string Start(StartDebuggingOptions options);

		/// <summary>
		/// true if <see cref="Restart"/> can be called
		/// </summary>
		public abstract bool CanRestart { get; }

		/// <summary>
		/// Restarts the debugged program(s)
		/// </summary>
		public abstract void Restart();

		/// <summary>
		/// true if a program is being debugged
		/// </summary>
		public abstract bool IsDebugging { get; }

		/// <summary>
		/// Raised when <see cref="IsDebugging"/> is changed
		/// </summary>
		public abstract event EventHandler IsDebuggingChanged;

		/// <summary>
		/// true if all processes are running, false if they're all paused, and null
		/// if some are running and some are paused.
		/// This property is valid only if <see cref="IsDebugging"/> is true.
		/// </summary>
		public abstract bool? IsRunning { get; }

		/// <summary>
		/// Raised when <see cref="IsRunning"/> is changed, see also <see cref="DelayedIsRunningChanged"/>
		/// </summary>
		public abstract event EventHandler IsRunningChanged;

		/// <summary>
		/// Raised when all processes have been running for a little while, eg. 1 second.
		/// </summary>
		public abstract event EventHandler DelayedIsRunningChanged;

		/// <summary>
		/// Gets all debug tags, see <see cref="PredefinedDebugTags"/>
		/// </summary>
		public abstract string[] DebugTags { get; }

		/// <summary>
		/// Raised when <see cref="DebugTags"/> is changed
		/// </summary>
		public abstract event EventHandler<DbgCollectionChangedEventArgs<string>> DebugTagsChanged;

		/// <summary>
		/// Gets all debugged processes. Can be empty even if <see cref="IsDebugging"/> is true
		/// if the process hasn't been created yet.
		/// </summary>
		public abstract DbgProcess[] Processes { get; }

		/// <summary>
		/// Raised when <see cref="Processes"/> is changed
		/// </summary>
		public abstract event EventHandler<DbgCollectionChangedEventArgs<DbgProcess>> ProcessesChanged;

		/// <summary>
		/// Pauses all debugged processes
		/// </summary>
		public abstract void BreakAll();

		/// <summary>
		/// Lets all programs run again. This is the inverse of <see cref="BreakAll"/>
		/// </summary>
		public abstract void RunAll();

		/// <summary>
		/// Stops debugging. All programs started by the debugger will be terminated. All
		/// other programs will be detached, if possible, else terminated.
		/// </summary>
		public abstract void StopDebuggingAll();

		/// <summary>
		/// Terminates all debugged programs
		/// </summary>
		public abstract void TerminateAll();

		/// <summary>
		/// Detaches all debugged programs, if possible. If it's not possible to detach a
		/// program, it will be terminated.
		/// </summary>
		public abstract void DetachAll();

		/// <summary>
		/// true if <see cref="DetachAll"/> can be called without terminating any programs
		/// </summary>
		public abstract bool CanDetachWithoutTerminating { get; }

		/// <summary>
		/// Gets the current process
		/// </summary>
		public abstract DbgCurrentObject<DbgProcess> CurrentProcess { get; }

		/// <summary>
		/// Raised when <see cref="CurrentProcess"/> is changed
		/// </summary>
		public abstract event EventHandler<DbgCurrentObjectChangedEventArgs<DbgProcess>> CurrentProcessChanged;

		/// <summary>
		/// Gets the current runtime
		/// </summary>
		public abstract DbgCurrentObject<DbgRuntime> CurrentRuntime { get; }

		/// <summary>
		/// Raised when <see cref="CurrentRuntime"/> is changed
		/// </summary>
		public abstract event EventHandler<DbgCurrentObjectChangedEventArgs<DbgRuntime>> CurrentRuntimeChanged;

		/// <summary>
		/// Gets the current thread
		/// </summary>
		public abstract DbgCurrentObject<DbgThread> CurrentThread { get; }

		/// <summary>
		/// Raised when <see cref="CurrentThread"/> is changed
		/// </summary>
		public abstract event EventHandler<DbgCurrentObjectChangedEventArgs<DbgThread>> CurrentThreadChanged;

		/// <summary>
		/// Returns true if the runtime can be debugged
		/// </summary>
		/// <param name="pid">Process id</param>
		/// <param name="rid">Runtime id</param>
		/// <returns></returns>
		public bool CanDebugRuntime(int pid, RuntimeId rid) => CanDebugRuntime((uint)pid, rid);

		/// <summary>
		/// Returns true if the runtime can be debugged
		/// </summary>
		/// <param name="pid">Process id</param>
		/// <param name="rid">Runtime id</param>
		/// <returns></returns>
		public abstract bool CanDebugRuntime(ulong pid, RuntimeId rid);

		/// <summary>
		/// Closes <paramref name="obj"/>
		/// </summary>
		/// <param name="obj">Object to close</param>
		public void Close(DbgObject obj) => Close(new[] { obj ?? throw new ArgumentNullException(nameof(obj)) });

		/// <summary>
		/// Closes <paramref name="objs"/>
		/// </summary>
		/// <param name="objs">Objects to close</param>
		public abstract void Close(DbgObject[] objs);
	}

	/// <summary>
	/// Contains the current object and the object that caused the debugger to enter break mode
	/// </summary>
	/// <typeparam name="T">Type of object</typeparam>
	public abstract class DbgCurrentObject<T> where T : DbgObject {
		/// <summary>
		/// Gets the current object or null if none
		/// </summary>
		public abstract T Current { get; set; }

		/// <summary>
		/// Gets the object that caused the debugger to enter break mode
		/// </summary>
		public abstract T Break { get; }
	}

	/// <summary>
	/// <see cref="DbgCurrentObject{T}"/> changed event args
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public struct DbgCurrentObjectChangedEventArgs<T> where T : DbgObject {
		/// <summary>
		/// true if <see cref="DbgCurrentObject{T}.Current"/> changed
		/// </summary>
		public bool CurrentChanged { get; }

		/// <summary>
		/// true if <see cref="DbgCurrentObject{T}.Break"/> changed
		/// </summary>
		public bool BreakChanged { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="currentChanged">true if <see cref="DbgCurrentObject{T}.Current"/> changed</param>
		/// <param name="breakChanged">true if <see cref="DbgCurrentObject{T}.Break"/> changed</param>
		public DbgCurrentObjectChangedEventArgs(bool currentChanged, bool breakChanged) {
			CurrentChanged = currentChanged;
			BreakChanged = breakChanged;
		}
	}
}
