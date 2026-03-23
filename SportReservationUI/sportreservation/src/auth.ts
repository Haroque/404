import { useRouter } from "./router";

export const API_URL: string = "http://localhost:5234/api"
export const CREDS: string = "creds"

export async function tryLogin(email: string, password: string): Promise<boolean> {
    const header = btoa(email + ':' + password)

    const res = await fetch(API_URL + "/User/Self", {
        headers: {
            'Authorization': 'Basic ' + header
        }
    })
    if (res.status == 401) {
        return false;
    }
    localStorage.setItem(CREDS, header)
    return true;
}

export async function logout() {
    localStorage.removeItem(CREDS)
    await useRouter().push({ name: 'login' })
}

export async function secureFetch(path: string, data: any): Promise<Response> {
    const header = localStorage.getItem(CREDS)

    if (header != null) {
        if (!data.headers) {
            data.headers = {};
        }
        data.headers['Authorization'] = 'Basic ' + header
        data.headers['Content-Type'] = "application/json"
    }

    const res = await fetch(API_URL + path, data);

    if (res.status == 401) {
        await useRouter().push({ name: 'login' })
        return res
    }
    return res
}