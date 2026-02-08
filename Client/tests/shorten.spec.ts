import { test, expect } from "@playwright/test";

const landingUrlField = "Paste a long URL (we add https://)";
const dashboardUrlField = "Paste long URL";
const redirectBaseUrl = process.env.E2E_REDIRECT_BASE_URL || "http://localhost:5011";

const getShortLink = (baseUrl: string, page: import("@playwright/test").Page) => {
  const host = new URL(baseUrl).host.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
  return page.getByRole("link", { name: new RegExp(`${host}/`) });
};

test.describe("URL shortener", () => {
  test("guest flow: rejects invalid domains and creates short URL", async ({ page }) => {
    await page.goto("/");

    await page.getByRole("textbox", { name: landingUrlField }).fill("somegoh.aeklgh.egq.qe");
    await page.getByRole("button", { name: "Shorten" }).click();
    await expect(page.getByText("Invalid URL format: invalid or unsupported domain")).toBeVisible();

    await page.getByRole("textbox", { name: landingUrlField }).fill("google.com");
    await page.getByRole("button", { name: "Shorten" }).click();
    await expect(page.getByText("URL shortened successfully!")).toBeVisible();

    const baseUrl = test.info().project.use.baseURL as string;
    const shortLink = getShortLink(baseUrl, page);
    await expect(shortLink).toBeVisible();

    const href = await shortLink.getAttribute("href");
    expect(href).toBeTruthy();
    expect(href).toMatch(/\/[0-9A-Za-z]{6,}$/);
  });

  test("guest flow: short link redirects", async ({ page }) => {
    await page.goto("/");

    await page.getByRole("textbox", { name: landingUrlField }).fill("example.com");
    await page.getByRole("button", { name: "Shorten" }).click();
    await expect(page.getByText("URL shortened successfully!")).toBeVisible();

    const baseUrl = test.info().project.use.baseURL as string;
    const shortLink = getShortLink(baseUrl, page);
    await expect(shortLink).toBeVisible();

    const href = await shortLink.getAttribute("href");
    expect(href).toBeTruthy();

    const shortCode = href!.split("/").pop();
    expect(shortCode).toBeTruthy();

    const redirectPage = await page.context().newPage();
    await redirectPage.goto(`${redirectBaseUrl}/${shortCode}`);
    await expect(redirectPage).toHaveURL(/https:\/\/example\.com\/?/);
    await redirectPage.close();
  });

  test("auth flow: sign up, create URL, logout", async ({ page }) => {
    await page.goto("/");

    await page.getByRole("button", { name: "Sign In" }).first().click();
    await page.getByRole("button", { name: "Don't have an account? Sign up" }).click();

    const timestamp = Date.now();
    const username = `user${timestamp}`;
    const email = `user${timestamp}@example.com`;
    const password = `Pass${timestamp}!`;

    await page.getByRole("textbox", { name: "Username" }).fill(username);
    await page.getByRole("textbox", { name: "Email" }).fill(email);
    await page.getByRole("textbox", { name: "Password" }).fill(password);
    await page.getByRole("button", { name: "Create Account" }).click();

    await expect(page.getByRole("heading", { name: "Create Short Link" })).toBeVisible();

    await page.getByRole("textbox", { name: dashboardUrlField }).fill("github.com");
    await page.getByRole("button", { name: "Create" }).click();
    await expect(page.getByText("URL created successfully!")).toBeVisible();

    await page.getByRole("button", { name: "Logout" }).click();
    await expect(page.getByRole("button", { name: "Sign In" })).toBeVisible();
    await expect(page.getByText("UrlShort")).toBeVisible();
  });

  test("auth flow: continue as guest closes modal", async ({ page }) => {
    await page.goto("/");

    await page.getByRole("button", { name: "Sign In" }).first().click();
    await expect(page.getByRole("heading", { name: "Welcome Back" })).toBeVisible();
    await page.getByRole("button", { name: "Continue as guest" }).click();

    await expect(page.getByRole("button", { name: "Sign In" })).toBeVisible();
    await expect(page.getByText("UrlShort")).toBeVisible();
  });

  test("auth flow: login with existing account", async ({ page }) => {
    await page.goto("/");

    await page.getByRole("button", { name: "Sign In" }).first().click();
    await page.getByRole("button", { name: "Don't have an account? Sign up" }).click();

    const timestamp = Date.now();
    const username = `user${timestamp}`;
    const email = `user${timestamp}@example.com`;
    const password = `Pass${timestamp}!`;

    await page.getByRole("textbox", { name: "Username" }).fill(username);
    await page.getByRole("textbox", { name: "Email" }).fill(email);
    await page.getByRole("textbox", { name: "Password" }).fill(password);
    await page.getByRole("button", { name: "Create Account" }).click();

    await expect(page.getByRole("heading", { name: "Create Short Link" })).toBeVisible();

    await page.getByRole("button", { name: "Logout" }).click();
    await expect(page.getByRole("button", { name: "Sign In" })).toBeVisible();

    await page.getByRole("button", { name: "Sign In" }).first().click();
    await page.getByRole("textbox", { name: "Email" }).fill(email);
    await page.getByRole("textbox", { name: "Password" }).fill(password);
    
    // Click the Sign In button inside the modal form (after Password field)
    await page.locator("form").getByRole("button", { name: "Sign In" }).click();

    await expect(page.getByRole("heading", { name: "Create Short Link" })).toBeVisible();
  });
});
