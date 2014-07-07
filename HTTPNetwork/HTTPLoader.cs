﻿using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using SimpleJSON;
using System;


//  HTTPLoader.cs
//  Author: Lu Zexi
//  2013-12-16

namespace Game.Network
{

    /// <summary>
    /// HTTP加载
    /// </summary>
	public class HTTPLoader : MonoBehaviour 
    {
		private STATE m_eState;
		private enum STATE
		{
			START = 0,
			STOP,
			CLOSE
		}

        /// <summary>
        /// Gos the WWW.
        /// </summary>
        /// <returns>The WW.</returns>
        /// <param name="url">URL.</param>
        /// <param name="form">Form.</param>
        /// <param name="error_callback">Error_callback.</param>
        /// <param name="callback2">Callback2.</param>
		public static void GoWWW<T>(
			string url , WWWForm form ,
			System.Action<string,System.Action,System.Action> error_callback ,
			System.Action<T> callback2
			)
			where T : HTTPPacketAck
        {
            GameObject obj = new GameObject("HTTPLoader");
            HTTPLoader loader = obj.AddComponent<HTTPLoader>();
			loader.m_eState = STATE.START;
            loader.StartCoroutine(loader.StartHTTP<T>(url , form , error_callback , callback2));
        }

		/// <summary>
		/// Res the send.
		/// </summary>
		public void ReSend()
		{
			this.m_eState = STATE.START;
		}

		/// <summary>
		/// Close this instance.
		/// </summary>
		public void Close()
		{
			this.m_eState = STATE.CLOSE;
		}

        /// <summary>
        /// Starts the HTTP.
        /// </summary>
        /// <returns>The HTT.</returns>
		public IEnumerator StartHTTP<T>(string url , WWWForm form ,
		                                System.Action<string,System.Action,System.Action> error_callback ,
		                                System.Action<T> callback2)
			where T : HTTPPacketAck
        {
			WWW www = null;
			bool ok = true;
			for( ; ok ;)
			{
				switch(this.m_eState)
				{
				case STATE.START:
					Debug.Log("send url " + url);
					url = System.Uri.EscapeUriString(url);
					if( form != null )
						www = new WWW(url , form);
					else
						www = new WWW(url);
					yield return www;
					try
					{
						if (www.error != null)
						{
							Debug.LogError("ERROR HTTP : " + www.error);
							this.m_eState = STATE.STOP;
							if(error_callback != null)
								error_callback(www.error,ReSend , Close);
						}
						else
						{
							Debug.Log(www.text);
							JSONNode obj = JSON.Parse(www.text);
							Debug.Log(obj.ToString());
							T packet = JsonPacker.Unpack(obj , typeof(T)) as T;
							this.m_eState = STATE.CLOSE;
							callback2(packet);
						}
					}
					catch( Exception ex )
					{
						this.m_eState = STATE.STOP;
						if(error_callback != null)
							error_callback(ex.StackTrace,ReSend , Close);
					}
					break;
				case STATE.STOP:
					yield return new WaitForFixedUpdate();
					break;
				case STATE.CLOSE:
					GameObject.Destroy(this.gameObject);
					ok = false;
					break;
				}
			}
        }

    }

}