// SPDX-License-Identifier: EUPL-1.2
// Copyright (c) Manuel Josupeit-Walter

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Josupeit.Practices.Schedulers;

/// <summary>
/// This interface represents a scheduler that schedules certain operations
/// for execution.
/// </summary>
[PublicAPI]
public interface IScheduler
{
    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed.
    /// </summary>
    /// <param name="action">The synchronous action to execute.</param>
    void Schedule(Action action);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed. When this instance is stopped,
    /// the <see cref="CancellationToken"/> supplied to the action will be
    /// canceled.
    /// </summary>
    /// <param name="cancellableAction">The synchronous action to execute.</param>
    void Schedule(Action<CancellationToken> cancellableAction);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed. When this instance is stopped
    /// or the supplied <paramref name="cancellationToken"/> is canceled,
    /// the <see cref="CancellationToken"/> supplied to the action will be
    /// canceled.
    /// </summary>
    /// <param name="cancellableAction">The synchronous action to execute.</param>
    /// <param name="cancellationToken">A token that cancels the execution.</param>
    void Schedule(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed.
    /// </summary>
    /// <param name="asyncAction">The asynchronous action to execute.</param>
    void Schedule(Func<Task> asyncAction);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed. When this instance is stopped,
    /// the <see cref="CancellationToken"/> supplied to the action will be
    /// canceled.
    /// </summary>
    /// <param name="cancellableAsyncAction">The cancellable asynchronous action.</param>
    void Schedule(Func<CancellationToken, Task> cancellableAsyncAction);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed. When this instance is stopped
    /// or the supplied <paramref name="cancellationToken"/> is canceled,
    /// the <see cref="CancellationToken"/> supplied to the action will be
    /// canceled.
    /// </summary>
    /// <param name="cancellableAsyncAction">The cancellable asynchronous action.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    void Schedule(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="func">The synchronous function to execute.</param>
    /// <returns>The result of the supplied function.</returns>
    TResult Schedule<TResult>(Func<TResult> func);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function. When this instance is stopped, the <see cref="CancellationToken"/>
    /// supplied to the function will be canceled.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cancellableFunc">The synchronous function to execute.</param>
    /// <returns>The result of the supplied function.</returns>
    TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function. When this instance is stopped or the supplied
    /// <paramref name="cancellationToken"/> is canceled, the
    /// <see cref="CancellationToken"/> supplied to the function will be canceled.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cancellableFunc">The synchronous function to execute.</param>
    /// <param name="cancellationToken">A token that cancels the execution.</param>
    /// <returns>The result of the supplied function.</returns>
    TResult Schedule<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="asyncFunc">The asynchronous function to execute.</param>
    /// <returns>The result of the supplied function.</returns>
    TResult Schedule<TResult>(Func<Task<TResult>> asyncFunc);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function. When this instance is stopped, the <see cref="CancellationToken"/>
    /// supplied to the function will be canceled.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cancellableAsyncFunc">The asynchronous function to execute.</param>
    /// <returns>The result of the supplied function.</returns>
    TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function. When this instance is stopped or the supplied
    /// <paramref name="cancellationToken"/> is canceled, the
    /// <see cref="CancellationToken"/> supplied to the function will be canceled.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cancellableAsyncFunc">The asynchronous function to execute.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>The result of the supplied function.</returns>
    TResult Schedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed.
    /// </summary>
    /// <param name="action">The synchronous action to execute.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous progress.</returns>
    Task ScheduleAsync(Action action);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed.
    /// </summary>
    /// <param name="action">The synchronous action to execute.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous progress.</returns>
    Task ScheduleAsync(Action action, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed. When this instance is stopped,
    /// the <see cref="CancellationToken"/> supplied to the action will be canceled.
    /// </summary>
    /// <param name="cancellableAction">The synchronous action to execute.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous progress.</returns>
    Task ScheduleAsync(Action<CancellationToken> cancellableAction);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed. When this instance is stopped or the supplied
    /// <paramref name="cancellationToken"/> is canceled, the <see cref="CancellationToken"/> 
    /// supplied to the action will be canceled.
    /// </summary>
    /// <param name="cancellableAction">The synchronous action to execute.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous progress.</returns>
    Task ScheduleAsync(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed.
    /// </summary>
    /// <param name="asyncAction">The asynchronous action to execute.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous progress.</returns>
    Task ScheduleAsync(Func<Task> asyncAction);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed.
    /// </summary>
    /// <param name="asyncAction">The asynchronous action to execute.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous progress.</returns>
    Task ScheduleAsync(Func<Task> asyncAction, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed. When this instance is stopped, the
    /// <see cref="CancellationToken"/> supplied to the action will be canceled.
    /// </summary>
    /// <param name="cancellableAsyncAction">The cancellable asynchronous action.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous progress.</returns>
    Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed. When this instance is stopped or the supplied 
    /// <paramref name="cancellationToken"/> is canceled, the <see cref="CancellationToken"/> 
    /// supplied to the action will be canceled.
    /// </summary>
    /// <param name="cancellableAsyncAction">The cancellable asynchronous action.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous progress.</returns>
    Task ScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="func">The synchronous function to execute.</param>
    /// <returns>The result of the supplied function encapsulated in a <see cref="Task{TResult}" />.</returns>
    Task<TResult> ScheduleAsync<TResult>(Func<TResult> func);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="func">The synchronous function to execute.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>The result of the supplied function encapsulated in a <see cref="Task{TResult}" />.</returns>
    Task<TResult> ScheduleAsync<TResult>(Func<TResult> func, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function. When this instance is stopped, the <see cref="CancellationToken"/>
    /// supplied to the function will be canceled.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cancellableFunc">The synchronous function to execute.</param>
    /// <returns>The result of the supplied function encapsulated in a <see cref="Task{TResult}" />.</returns>
    Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function. When this instance is stopped or the supplied <paramref name="cancellationToken"/>
    /// is canceled, the <see cref="CancellationToken"/> supplied to the function will be canceled.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cancellableFunc">The synchronous function to execute.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>The result of the supplied function encapsulated in a <see cref="Task{TResult}" />.</returns>
    Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="asyncFunc">The asynchronous function to execute.</param>
    /// <returns>The result of the supplied function encapsulated in a <see cref="Task{TResult}" />.</returns>
    Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="asyncFunc">The asynchronous function to execute.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>The result of the supplied function encapsulated in a <see cref="Task{TResult}" />.</returns>
    Task<TResult> ScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function. When this instance is stopped, the <see cref="CancellationToken"/>
    /// supplied to the function will be canceled.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cancellableAsyncFunc">The asynchronous function to execute.</param>
    /// <returns>The result of the supplied function encapsulated in a <see cref="Task{TResult}" />.</returns>
    Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function. When this instance is stopped or the supplied <paramref name="cancellationToken"/>
    /// is canceled, the <see cref="CancellationToken"/> supplied to the function will be canceled.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cancellableAsyncFunc">The asynchronous function to execute.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>The result of the supplied function encapsulated in a <see cref="Task{TResult}" />.</returns>
    Task<TResult> ScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <param name="action">The synchronous action to execute.</param>
    void UnsafeSchedule(Action action);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed. When this instance is stopped,
    /// the <see cref="CancellationToken"/> supplied to the action will be
    /// canceled.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <param name="cancellableAction">The synchronous action to execute.</param>
    void UnsafeSchedule(Action<CancellationToken> cancellableAction);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed. When this instance is stopped
    /// or the supplied <paramref name="cancellationToken"/> is canceled,
    /// the <see cref="CancellationToken"/> supplied to the action will be
    /// canceled.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <param name="cancellableAction">The synchronous action to execute.</param>
    /// <param name="cancellationToken">A token that cancels the execution.</param>
    void UnsafeSchedule(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <param name="asyncAction">The asynchronous action to execute.</param>
    void UnsafeSchedule(Func<Task> asyncAction);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed. When this instance is stopped,
    /// the <see cref="CancellationToken"/> supplied to the action will be
    /// canceled.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <param name="cancellableAsyncAction">The cancellable asynchronous action.</param>
    void UnsafeSchedule(Func<CancellationToken, Task> cancellableAsyncAction);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed. When this instance is stopped
    /// or the supplied <paramref name="cancellationToken"/> is canceled,
    /// the <see cref="CancellationToken"/> supplied to the action will be
    /// canceled.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <param name="cancellableAsyncAction">The cancellable asynchronous action.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    void UnsafeSchedule(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="func">The synchronous function to execute.</param>
    /// <returns>The result of the supplied function.</returns>
    TResult UnsafeSchedule<TResult>(Func<TResult> func);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function. When this instance is stopped, the <see cref="CancellationToken"/>
    /// supplied to the function will be canceled.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cancellableFunc">The synchronous function to execute.</param>
    /// <returns>The result of the supplied function.</returns>
    TResult UnsafeSchedule<TResult>(Func<CancellationToken, TResult> cancellableFunc);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function. When this instance is stopped or the supplied
    /// <paramref name="cancellationToken"/> is canceled, the
    /// <see cref="CancellationToken"/> supplied to the function will be canceled.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cancellableFunc">The synchronous function to execute.</param>
    /// <param name="cancellationToken">A token that cancels the execution.</param>
    /// <returns>The result of the supplied function.</returns>
    TResult UnsafeSchedule<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="asyncFunc">The asynchronous function to execute.</param>
    /// <returns>The result of the supplied function.</returns>
    TResult UnsafeSchedule<TResult>(Func<Task<TResult>> asyncFunc);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function. When this instance is stopped, the <see cref="CancellationToken"/>
    /// supplied to the function will be canceled.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cancellableAsyncFunc">The asynchronous function to execute.</param>
    /// <returns>The result of the supplied function.</returns>
    TResult UnsafeSchedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function. When this instance is stopped or the supplied
    /// <paramref name="cancellationToken"/> is canceled, the
    /// <see cref="CancellationToken"/> supplied to the function will be canceled.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cancellableAsyncFunc">The asynchronous function to execute.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>The result of the supplied function.</returns>
    TResult UnsafeSchedule<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <param name="action">The synchronous action to execute.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous progress.</returns>
    Task UnsafeScheduleAsync(Action action);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <param name="action">The synchronous action to execute.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous progress.</returns>
    Task UnsafeScheduleAsync(Action action, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed. When this instance is stopped,
    /// the <see cref="CancellationToken"/> supplied to the action will be canceled.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <param name="cancellableAction">The synchronous action to execute.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous progress.</returns>
    Task UnsafeScheduleAsync(Action<CancellationToken> cancellableAction);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed. When this instance is stopped or the supplied
    /// <paramref name="cancellationToken"/> is canceled, the <see cref="CancellationToken"/> 
    /// supplied to the action will be canceled.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <param name="cancellableAction">The synchronous action to execute.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous progress.</returns>
    Task UnsafeScheduleAsync(Action<CancellationToken> cancellableAction, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <param name="asyncAction">The asynchronous action to execute.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous progress.</returns>
    Task UnsafeScheduleAsync(Func<Task> asyncAction);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <param name="asyncAction">The asynchronous action to execute.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous progress.</returns>
    Task UnsafeScheduleAsync(Func<Task> asyncAction, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed. When this instance is stopped, the
    /// <see cref="CancellationToken"/> supplied to the action will be canceled.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <param name="cancellableAsyncAction">The cancellable asynchronous action.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous progress.</returns>
    Task UnsafeScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction);

    /// <summary>
    /// Schedules the specified action to be executed. This method terminates
    /// after the action has been completed. When this instance is stopped or the supplied 
    /// <paramref name="cancellationToken"/> is canceled, the <see cref="CancellationToken"/> 
    /// supplied to the action will be canceled.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <param name="cancellableAsyncAction">The cancellable asynchronous action.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous progress.</returns>
    Task UnsafeScheduleAsync(Func<CancellationToken, Task> cancellableAsyncAction, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="func">The synchronous function to execute.</param>
    /// <returns>The result of the supplied function encapsulated in a <see cref="Task{TResult}" />.</returns>
    Task<TResult> UnsafeScheduleAsync<TResult>(Func<TResult> func);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="func">The synchronous function to execute.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>The result of the supplied function encapsulated in a <see cref="Task{TResult}" />.</returns>
    Task<TResult> UnsafeScheduleAsync<TResult>(Func<TResult> func, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function. When this instance is stopped, the <see cref="CancellationToken"/>
    /// supplied to the function will be canceled.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cancellableFunc">The synchronous function to execute.</param>
    /// <returns>The result of the supplied function encapsulated in a <see cref="Task{TResult}" />.</returns>
    Task<TResult> UnsafeScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function. When this instance is stopped or the supplied <paramref name="cancellationToken"/>
    /// is canceled, the <see cref="CancellationToken"/> supplied to the function will be canceled.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cancellableFunc">The synchronous function to execute.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>The result of the supplied function encapsulated in a <see cref="Task{TResult}" />.</returns>
    Task<TResult> UnsafeScheduleAsync<TResult>(Func<CancellationToken, TResult> cancellableFunc, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="asyncFunc">The asynchronous function to execute.</param>
    /// <returns>The result of the supplied function encapsulated in a <see cref="Task{TResult}" />.</returns>
    Task<TResult> UnsafeScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="asyncFunc">The asynchronous function to execute.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>The result of the supplied function encapsulated in a <see cref="Task{TResult}" />.</returns>
    Task<TResult> UnsafeScheduleAsync<TResult>(Func<Task<TResult>> asyncFunc, CancellationToken cancellationToken);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function. When this instance is stopped, the <see cref="CancellationToken"/>
    /// supplied to the function will be canceled.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cancellableAsyncFunc">The asynchronous function to execute.</param>
    /// <returns>The result of the supplied function encapsulated in a <see cref="Task{TResult}" />.</returns>
    Task<TResult> UnsafeScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc);

    /// <summary>
    /// Schedules the specified function to be executed. This method terminates
    /// after the function has been completed and returns the result of the supplied
    /// function. When this instance is stopped or the supplied <paramref name="cancellationToken"/>
    /// is canceled, the <see cref="CancellationToken"/> supplied to the function will be canceled.
    /// </summary>
    /// <remarks>This method does not capture the current execution context.</remarks>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="cancellableAsyncFunc">The asynchronous function to execute.</param>
    /// <param name="cancellationToken">A token that cancels the asynchronous execution.</param>
    /// <returns>The result of the supplied function encapsulated in a <see cref="Task{TResult}" />.</returns>
    Task<TResult> UnsafeScheduleAsync<TResult>(Func<CancellationToken, Task<TResult>> cancellableAsyncFunc, CancellationToken cancellationToken);
}
