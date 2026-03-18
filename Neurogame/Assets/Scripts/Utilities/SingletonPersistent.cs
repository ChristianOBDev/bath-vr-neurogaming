/*
 * Copyright (c) 2026 NeuroCONCISE
 * All rights reserved.
 *
 * Permission is hereby granted to use, copy, and modify this software
 * for personal or internal purposes, provided that this copyright
 * notice and this permission notice appear in all copies.
 *
 * Redistribution, sublicensing, or commercial use of this software,
 * in source or binary form, is prohibited without prior written
 * permission from the copyright holder.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */

using UnityEngine;

public abstract class SingletonPersistent<T> : Singleton<T>
    where T : SingletonPersistent<T>
{
    protected override void Awake()
    {
        base.Awake();

        // If this instance survived the base Awake (i.e., not destroyed)
        if (Instance == this)
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}