import { useDispatch, useStore } from "react-redux";
import { useEffect } from "react";
import { getConnection, ensureConnection, startSignalR } from "../../services/signalrService.js";
import { registerListeners } from "./remoteUtils.js";

export function useRemoteSignalR() {
  const dispatch = useDispatch();
  const store = useStore();

  useEffect(() => {
    let cleanup = () => {};

    const conn = ensureConnection();
    cleanup = registerListeners(conn, dispatch, store.getState);

    startSignalR();

    return () => {
      cleanup();
    };
  }, [dispatch, store]);
}
