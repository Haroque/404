export abstract class Form {
    public opened = false
    public valid = false
    public loading = false
    public error = false
    public errorMessage = ""

    public open(data: any = null) {
        this.opened = true
        this.onOpen(data)
    }

    public close() {
        this.opened = false
        this.error = false
        this.errorMessage = ""
        this.onClear()
    }

    public post() {
        this.loading = true

        this.onPost().then(res => {
            if (res) {
                this.onReload().then(() => {
                    this.close()
                })
            }
            this.loading = false
        })
    }

    public fail(message: string) {
        this.error = true
        this.errorMessage = message
    }

    public abstract onClear(): void

    public onOpen(data: any): void {

    }

    public async onReload(): Promise<void> {

    }

    public async onPost(): Promise<boolean> {
        return false
    }
}

export function isNotNullOrEmpty(value: string) {
    return value != null && value.length > 0
}

// constraints

export function required(v: any) {
    return !!v || "Tohle pole je povinné"
}