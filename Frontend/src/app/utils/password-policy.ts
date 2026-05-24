export const PASSWORD_POLICY_MESSAGE = 'La contraseña debe tener al menos 8 caracteres, una mayúscula, un número y un símbolo.';

const PASSWORD_POLICY_REGEX = /^(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]).{8,}$/;

export function isPasswordValid(password: string): boolean {
  return PASSWORD_POLICY_REGEX.test(password);
}
