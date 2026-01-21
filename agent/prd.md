# Product Requirements Document (PRD)

## Overview

A minimal URL shortener service, similar to Bitly, supporting both anonymous and registered users. Registered users have
access to analytics and custom aliases. Admins have full access to analytics and management features.

## Core Entities

- **User**: id, email, createdAt, updatedAt
- **URL**: id, userId, long, short (alias, unique), createdAt, expiry
- **Visit**: id, urlId, click metadata (user agent, IP, geo if available)
- **Analytics**: id, statDate, statHour, visits, country (rollup of visits)

## Features

- Anonymous and registered URL shortening
- Registered users: analytics, custom aliases
- Admin: full analytics, CRUD on all URLs/users
- Expiry option for URLs
- RESTful API endpoints
- Rate limiting to prevent abuse
- Redis cache for fast shortcode resolution (scaling later)

## Out of Scope (for now)

- QR code generation
- Custom domains
- Bulk URL shortening

## Technical Notes

- Use a global counter for unique shortcodes (DB constraint on alias)
- Visit tracking: user agent, IP, geo (if possible)
- Standard RESTful API
- Redis for caching
- Admin features to be implemented later
