import {useRouter} from "./router";

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

    const json = await res.json()

    if (json.role == 'Admin') {
        await useRouter().push({name: 'admin'})
        return true
    }
    await useRouter().push({name: 'home'})
    return true
}

export async function logout() {
    localStorage.removeItem(CREDS)
    await useRouter().push({name: 'login'})
}

export async function secureFetch(path: string, data: any = {}): Promise<Response> {
    const header = localStorage.getItem(CREDS)

    if (!data.headers) {
        data.headers = {};
    }
    
    // Always set Content-Type for JSON requests
    if (data.body) {
        data.headers['Content-Type'] = "application/json";
    }

    if (header != null) {
        data.headers['Authorization'] = 'Basic ' + header
    }

    const fullUrl = API_URL + path;
    console.log('secureFetch calling:', fullUrl);
    console.log('Request data:', data);
    
    try {
        const res = await fetch(fullUrl, data);
        console.log('secureFetch response status:', res.status);

        if (res.status == 401) {
            await useRouter().push({name: 'login'})
            return res
        }
        if (res.status == 403) {
            await useRouter().push({name: 'home'})
            return res
        }
        return res
    } catch (error) {
        console.error('secureFetch error:', error);
        throw error;
    }
}