export class Credentials {
  constructor(
    public login: string,
    public password: string,
    public recaptchaToken: string,
    public action: string
  ) { }
}
